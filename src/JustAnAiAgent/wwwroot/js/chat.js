var chatTitle = document.getElementById('title');
var messagesBox = document.getElementById('messages-box');
var conversationsList = document.getElementById('conversations-list');
var projectsList = document.getElementById('projects-list');
var messageInput = document.getElementById('message-input');
var currentActionSpan = document.getElementById('current-action-span');
var modelsSelect = document.getElementById('model-select');
var modelSelectButton = document.getElementById('model-select-button');
var selectedModel = document.getElementById('selected-model');
var selectedSendMode = document.getElementById('selected-send-mode');
var sendModeMenu = document.getElementById('send-mode-menu');
var sendMessageButton = document.getElementById('send-message-button');
var loadMoreConversationsButton = document.getElementById('load-more-conversations');
var loadMoreProjectsButton = document.getElementById('load-more-projects');

var newConversationName = document.getElementById('new-conversation-name');
var newConversationDescription = document.getElementById('new-conversation-description');
var newConversationAddButton = document.getElementById('create-conversation');

var pageSize = 25;
var activeConversation = null;
var activeProject = null;
var activeProjectConversations = [];
var selectedModelId = null;
var selectedMode = { type: 'chat' };
var conversationsSkip = 0;
var projectsSkip = 0;
var hasMoreConversations = false;
var hasMoreProjects = false;
var loadedProjects = [];
var activeMessagesODataUrl = null;
var messageLimit = pageSize;
var renderedMessages = new Map();
var renderedMessageOrder = [];

async function loadConversations(reset = true) {
    setCurrentAction('Loading conversations...');

    if (reset) {
        conversationsSkip = 0;
        conversationsList.innerHTML = '';
    }

    var response = await api.get(`Conversation?$orderBy=LastmessageSentAt desc&$skip=${conversationsSkip}&$top=${pageSize + 1}`);
    var conversations = getODataItems(response);
    hasMoreConversations = conversations.length > pageSize;
    conversations = conversations.slice(0, pageSize);
    conversationsSkip += conversations.length;

    conversationsList.append(...conversations.map(renderConversationListItem));
    loadMoreConversationsButton.classList.toggle('d-none', !hasMoreConversations);

    setCurrentActionIdle();

    return conversations;
}

async function loadProjects(reset = true) {
    if (!projectsList)
        return [];

    setCurrentAction('Loading projects...');

    if (reset) {
        projectsSkip = 0;
        projectsList.innerHTML = '';
        loadedProjects = [];
    }

    if (!activeConversation) {
        renderNoConnectedProjects();
        loadMoreProjectsButton?.classList.add('d-none');
        setCurrentActionIdle();
        return [];
    }

    var filter = `ConversationId eq ${activeConversation.Id}`;

    if (activeConversation.ProjectId)
        filter = `${filter} or Id eq ${activeConversation.ProjectId}`;

    var response = await api.get(`AgenticProject?$filter=${filter}&$orderBy=UpdatedAt desc,CreatedAt desc&$skip=${projectsSkip}&$top=${pageSize + 1}`);
    var projects = getODataItems(response);
    hasMoreProjects = projects.length > pageSize;
    projects = projects.slice(0, pageSize);
    projectsSkip += projects.length;
    loadedProjects.push(...projects);

    if (projects.length == 0 && reset)
        renderNoConnectedProjects();
    else
        projectsList.append(...projects.map(renderProjectListItem));

    if (loadMoreProjectsButton)
        loadMoreProjectsButton.classList.toggle('d-none', !hasMoreProjects);
    refreshSendModeOptions();

    setCurrentActionIdle();

    return projects;
}

async function loadConversation(id) {
    setCurrentAction('Loading conversation...');

    activeConversation = await api.get(`Conversation/${id}?$expand=AgenticProject`);
    activeProject = await resolveConversationProject(activeConversation);
    activeProjectConversations = activeProject ? [activeConversation] : [];
    messageLimit = pageSize;

    chatTitle.innerText = `JustAnAiAgent | ${activeConversation.Name}`;
    setActiveMessagesSource(`Message?$filter=ConversationId eq ${activeConversation.Id}&$orderby=CreatedAt desc`);
    await loadMessagesFromActiveSource();

    await setModelFromLastMessage(activeConversation.Id);
    await loadProjects();
    refreshSendModeOptions();
    defaultSendModeForActiveConversation();

    setCurrentActionIdle();
}

function renderNoConnectedProjects() {
    var item = makeElementWithClasses('li', ['list-group-item', 'text-secondary']);
    item.innerText = 'No connected projects';
    projectsList.appendChild(item);
}

async function loadProject(id) {
    setCurrentAction('Loading project...');

    activeProject = await api.get(`AgenticProject/${id}`);
    activeProjectConversations = getODataItems(await api.get(`Conversation?$filter=ProjectId eq ${id}&$orderBy=LastmessageSentAt desc&$top=${pageSize}`));

    if (activeProjectConversations.length == 0) {
        activeConversation = null;
        messageLimit = pageSize;
        chatTitle.innerText = `JustAnAiAgent | ${activeProject.Name}`;
        setActiveMessagesSource(null);
        clearMessages();
        refreshSendModeOptions();
        setSendMode({ type: 'new-project-agent', project: activeProject });
        setCurrentActionIdle();
        return;
    }

    activeConversation = activeProjectConversations[0];
    messageLimit = pageSize;
    chatTitle.innerText = `JustAnAiAgent | ${activeProject.Name}`;
    setActiveMessagesSource(`Message?$filter=Conversation/ProjectId eq ${activeProject.Id}&$orderby=CreatedAt desc`);
    await loadMessagesFromActiveSource();

    await setModelFromLastMessage(activeConversation.Id);
    refreshSendModeOptions();
    setSendMode({ type: 'project-agent', project: activeProject, conversation: activeConversation });

    setCurrentActionIdle();
}

function setActiveMessagesSource(odataUrl) {
    activeMessagesODataUrl = odataUrl;
}

async function loadMessagesFromActiveSource() {
    if (!activeMessagesODataUrl) {
        clearMessages();
        return;
    }

    var messages = getODataItems(await api.get(`${activeMessagesODataUrl}&$top=${messageLimit + 1}`));
    var hasMoreMessages = messages.length > messageLimit;
    messages = messages.slice(0, messageLimit).reverse();

    clearMessages();

    if (hasMoreMessages)
        messagesBox.appendChild(renderLoadMoreMessagesButton());

    for (var message of messages)
        addMessageToMessagesBox(message, getPerspectiveForMessage(message), true, false);

    messagesBox.scrollTo({
        top: messagesBox.scrollHeight,
        left: 0,
        behaviour: 'smooth'
    });
}

function clearMessages() {
    messagesBox.innerHTML = '';
    renderedMessages.clear();
    renderedMessageOrder = [];
}

async function loadMoreMessages() {
    messageLimit += pageSize;
    await loadMessagesFromActiveSource();
}

function renderLoadMoreMessagesButton() {
    var item = makeElementWithClasses('li', ['message-load-more', 'mb-2'], [
        makeElementWithClasses('button', ['btn', 'btn-sm', 'btn-outline-secondary', 'w-100'])
    ]);
    item.firstChild.innerText = 'Load more';
    item.firstChild.setAttribute('type', 'button');
    item.firstChild.setAttribute('data-action', 'load-more-messages');

    return item;
}

function renderConversationListItem(conversation) {
    var listItem = makeElementWithClasses('li', ['list-group-item', 'd-flex', 'justify-content-between', 'align-items-start', 'conversation']);

    var link = makeElementWithClasses('div', ['ms-2', 'me-auto']);
    link.setAttribute('data-action', 'load-conversation');
    link.setAttribute('data-conversation-id', conversation.Id);
    link.innerText = conversation.Name;

    var deleteButton = makeElementWithClasses('button', ['badge', 'text-bg-danger']);
    deleteButton.setAttribute('data-action', 'delete-conversation');
    deleteButton.setAttribute('data-conversation-id', conversation.Id);
    deleteButton.innerText = 'X';

    listItem.append(link, deleteButton);

    return listItem;
}

function renderProjectListItem(project) {
    var listItem = makeElementWithClasses('li', ['list-group-item', 'project']);
    listItem.setAttribute('data-action', 'open-project');
    listItem.setAttribute('data-project-id', project.Id);
    listItem.innerText = project.Name;

    return listItem;
}

async function loadModels() {
    setCurrentAction('Loading available models...');
    var models = await api.get('Models');

    var splitModels = models.map(m => splitModelId(m));

    var groupedModels = splitModels.reduce((acc, model) => {
        if (!acc[model.provider]) {
            acc[model.provider] = [];
        }
        acc[model.provider].push(model);
        return acc;
    }, {});

    modelsSelect.innerHTML = '';

    for (var provider in groupedModels) {
        var divider = document.createElement('li');
        divider.classList.add('dropdown-divider');

        var providerSpan = document.createElement('li');
        providerSpan.innerHTML = `<span class="dropdown-item disabled">${provider}</span>`;

        if (modelsSelect.innerHTML != '')
            modelsSelect.appendChild(divider);

        modelsSelect.appendChild(providerSpan);
        modelsSelect.appendChild(divider);

        for (var model of groupedModels[provider]) {
            var item = document.createElement('li');
            item.innerHTML = `<a class="dropdown-item" href="#" onclick="setActiveModel('${model.id}')">${model.model}</a>`;
            modelsSelect.appendChild(item);
        }
    }

    setCurrentActionIdle();

    return groupedModels;
}

async function sendMessage() {
    var targetConversation = await getTargetConversationForSelectedMode();

    if (targetConversation == null) {
        window.alert('Select a conversation before sending a message.');
        return;
    }

    var message = messageInput.value.trim();

    if (!message)
        return;

    if (activeConversation?.Id != targetConversation.Id)
        await loadConversation(targetConversation.Id);

    addMessageToMessagesBox({
        Content: message,
        CreatedAt: new Date().toISOString(),
        ModelId: selectedModelId
    }, 'user');

    pruneRenderedMessagesToLimit();
    setCurrentAction('Waiting for model response...');

    messageInput.disabled = true;
    sendMessageButton.disabled = true;

    try {
        await api.postStream(`Chat/ConversationWithNewMessageStream/${targetConversation.Id}`, {
            ConversationId: targetConversation.Id,
            Content: message,
            ModelId: selectedModelId,
            Type: 'user',
            ContentType: 'string'
        }, upsertMessageToMessagesBox);

        messageInput.value = '';
        await loadConversations();
    } catch (error) {
        console.error(error);
        window.alert(`Failed to send message: ${error.message}`);
    } finally {
        messageInput.disabled = false;
        sendMessageButton.disabled = false;
        setCurrentActionIdle();
    }
}

async function getTargetConversationForSelectedMode() {
    if (selectedMode.type == 'chat')
        return activeConversation;

    if (selectedMode.type == 'project-agent')
        return selectedMode.conversation ?? activeConversation;

    if (selectedMode.type == 'new-project-agent') {
        var project = selectedMode.project ?? activeProject ?? getActiveConversationProject();

        if (!project) {
            window.alert('The current conversation is not tied to a project.');
            return null;
        }

        var conversation = await api.post('Conversation', {
            Name: `${project.Name} agent`,
            Description: `Project agent conversation for ${project.Name}`,
            ProjectId: project.Id
        });

        activeProjectConversations.unshift(conversation);
        setSendMode({ type: 'project-agent', project, conversation });

        return conversation;
    }

    return activeConversation;
}

function refreshSendModeOptions() {
    sendModeMenu.innerHTML = '';
    sendModeMenu.appendChild(makeSendModeMenuItem('Send to chat', { type: 'chat' }));

    var project = activeProject ?? getActiveConversationProject();

    if (project) {
        var projectConversation = activeProjectConversations.find(conversation => conversation.ProjectId == project.Id)
            ?? (activeConversation?.ProjectId == project.Id ? activeConversation : null);
        sendModeMenu.appendChild(makeSendModeMenuItem(`Send to existing project agent (${project.Name})`, {
            type: 'project-agent',
            project,
            conversation: projectConversation
        }));
    }

    sendModeMenu.appendChild(makeSendModeMenuItem('Send to new project agent', {
        type: 'new-project-agent',
        project
    }));

    updateSelectedSendModeText();
}

function makeSendModeMenuItem(label, mode) {
    var listItem = document.createElement('li');
    var button = makeElementWithClasses('button', ['dropdown-item']);
    button.setAttribute('type', 'button');
    button.innerText = label;
    button.addEventListener('click', function () {
        setSendMode(mode);
    });

    listItem.appendChild(button);

    return listItem;
}

function defaultSendModeForActiveConversation() {
    var project = activeProject ?? getActiveConversationProject();

    if (project && activeConversation?.ProjectId == project.Id) {
        setSendMode({
            type: 'project-agent',
            project,
            conversation: activeConversation
        });
        return;
    }

    setSendMode({ type: 'chat' });
}

function setSendMode(mode) {
    selectedMode = mode;
    updateSelectedSendModeText();
}

function updateSelectedSendModeText() {
    if (selectedMode.type == 'project-agent') {
        selectedSendMode.innerText = `Sending to existing agent (${selectedMode.project?.Name ?? 'project'})`;
        return;
    }

    if (selectedMode.type == 'new-project-agent') {
        selectedSendMode.innerText = 'Sending to new project agent';
        return;
    }

    selectedSendMode.innerText = 'Sending to chat';
}

function getActiveConversationProject() {
    if (!activeConversation?.ProjectId)
        return null;

    return activeConversation.AgenticProject
        ?? loadedProjects.find(project => project.Id == activeConversation.ProjectId)
        ?? null;
}

async function resolveConversationProject(conversation) {
    if (!conversation?.ProjectId)
        return null;

    return conversation.AgenticProject
        ?? loadedProjects.find(project => project.Id == conversation.ProjectId)
        ?? await api.get(`AgenticProject/${conversation.ProjectId}`);
}

async function setModelFromLastMessage(conversationId) {
    var messages = getODataItems(await api.get(`Message?$filter=ConversationId eq ${conversationId}&$orderby=CreatedAt desc&$top=1`));

    if (messages.length > 0)
        setActiveModel(messages[0]?.ModelId);
}

function setActiveModel(id) {
    selectedModelId = id;

    if (id == null) {
        selectedModel.innerText = 'No model selected';
        return;
    }

    var split = splitModelId(id);
    selectedModel.innerText = `${split.provider} | ${split.model}`;
}

function addMessageToMessagesBox(message, perspective, showStats = true, enforceLimit = true) {
    var renderedMessage = null;

    if (!perspective)
        return;

    var startIndex = messagesBox.children.length;

    if (perspective == 'user')
        renderedMessage = renderUserMessage(message);

    if (perspective == 'model-thought')
        renderedMessage = renderModelThought(message);

    if (perspective == 'model-response')
        renderedMessage = renderModelResponse(message, showStats);

    if (perspective == 'tool-calls')
        renderedMessage = renderToolCalls(message);

    if (perspective == 'tool-responses')
        renderedMessage = renderToolResponses(message);

    var renderedElements = Array.from(messagesBox.children).slice(startIndex);

    if (renderedMessage)
        renderedMessage.elements = renderedElements;

    if (message.Id && renderedMessage) {
        renderedMessages.set(message.Id, renderedMessage);
        renderedMessageOrder = renderedMessageOrder.filter(id => id != message.Id);
        renderedMessageOrder.push(message.Id);
    }

    if (enforceLimit)
        pruneRenderedMessagesToLimit();

    messagesBox.scrollTo({
        top: messagesBox.scrollHeight,
        left: 0,
        behaviour: 'smooth'
    });
}

function pruneRenderedMessagesToLimit() {
    while (renderedMessageOrder.length > messageLimit) {
        var id = renderedMessageOrder.shift();
        var renderedMessage = renderedMessages.get(id);

        if (!renderedMessage)
            continue;

        for (var element of renderedMessage.elements ?? [])
            element.remove();

        renderedMessages.delete(id);
    }
}

function upsertMessageToMessagesBox(message) {
    var renderedMessage = renderedMessages.get(message.Id);

    if (!renderedMessage) {
        addMessageToMessagesBox(message, getPerspectiveForMessage(message));
        return;
    }

    updateRenderedMessage(renderedMessage, message);

    messagesBox.scrollTo({
        top: messagesBox.scrollHeight,
        left: 0,
        behaviour: 'smooth'
    });
}

function getPerspectiveForMessage(message) {
    switch (message.Type) {
        case 'user':
            return 'user';

        case 'thought':
            return 'model-thought';

        case 'response':
            return 'model-response';

        case 'tool-calls':
            return 'tool-calls';

        case 'tool-results':
            return 'tool-responses';

        default:
            return null;
    }
}

function updateRenderedMessage(renderedMessage, message) {
    if (renderedMessage.receivedAtItem)
        renderedMessage.receivedAtItem.innerText = formatDate(message.ResponseReceivedAt);

    updateTokenUsageItem(renderedMessage, message);

    if (message.Type == 'response' && message.IsStillRunning)
        setCurrentAction('Streaming response...');

    if (renderedMessage.contentItem)
        setMessageContent(renderedMessage.contentItem, message, renderedMessage.html);
}

function renderUserMessage(message) {
    var statsItem = makeElementWithClasses('li', ['d-flex', 'justify-content-end'], [
        makeListGroup([
            makeListItem(formatDate(message.CreatedAt))
        ], ['list-group-horizontal'])
    ]);

    var contentItem = makeListItem(message.Content ?? '', ['list-group-item-info']);
    var messageItem = makeElementWithClasses('li', ['d-flex', 'justify-content-end'], [
        makeListGroup([
            contentItem
        ])
    ]);

    messagesBox.append(statsItem, messageItem);

    return {
        contentItem,
        html: false
    };
}

function renderModelThought(message) {
    var modelIdSplit = splitModelId(message.ModelId);
    var receivedAtItem = makeListItem(formatDate(message.ResponseReceivedAt));
    var tokenUsageItem = makeTokenUsageItem(message);
    var statItems = [
        receivedAtItem,
        makeListItem(modelIdSplit.provider),
        makeListItem(modelIdSplit.model)
    ];

    if (tokenUsageItem)
        statItems.push(tokenUsageItem);

    var statsListGroup = makeListGroup(statItems, ['list-group-horizontal']);

    var statsItem = makeElementWithClasses('li', [], [
        statsListGroup
    ]);

    var contentItem = makeListItem(message.Content ?? '', ['list-group-item-light']);
    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            contentItem
        ])
    ]);

    messagesBox.append(statsItem, messageItem);

    return {
        receivedAtItem,
        tokenUsageItem,
        statsListGroup,
        contentItem,
        html: false
    };
}

function renderModelResponse(message, renderStats = true) {
    var receivedAtItem = null;
    var tokenUsageItem = null;
    var statsListGroup = null;

    if (renderStats) {
        var modelIdSplit = splitModelId(message.ModelId);
        receivedAtItem = makeListItem(formatDate(message.ResponseReceivedAt));
        tokenUsageItem = makeTokenUsageItem(message);
        var statItems = [
            receivedAtItem,
            makeListItem(modelIdSplit.provider),
            makeListItem(modelIdSplit.model)
        ];

        if (tokenUsageItem)
            statItems.push(tokenUsageItem);

        statsListGroup = makeListGroup(statItems, ['list-group-horizontal']);

        var statsItem = makeElementWithClasses('li', [], [
            statsListGroup
        ]);

        messagesBox.appendChild(statsItem);
    }

    var contentItem = makeListItem(marked.parse(message.Content ?? ''), ['list-group-item-dark'], true);
    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            contentItem
        ])
    ]);

    messagesBox.appendChild(messageItem);

    return {
        receivedAtItem,
        tokenUsageItem,
        statsListGroup,
        contentItem,
        html: true
    };
}

function renderToolCalls(message) {
    var titleBar = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem('Using MCP tools...')
        ], ['list-group-horizontal'])
    ]);

    var contentItem = makeListItem(getDisplayContent(message), ['list-group-item-primary'], true);
    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            contentItem
        ])
    ]);

    messagesBox.append(titleBar, messageItem);

    return {
        contentItem,
        html: true
    };
}

function renderToolResponses(message) {
    var titleBar = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem('Tool responses')
        ], ['list-group-horizontal'])
    ]);

    var contentItem = makeListItem(getDisplayContent(message), ['list-group-item-success'], true);
    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            contentItem
        ])
    ]);

    messagesBox.append(titleBar, messageItem);

    return {
        contentItem,
        html: true
    };
}

function setMessageContent(contentItem, message, html) {
    var content = getDisplayContent(message);

    if (html)
        contentItem.innerHTML = content;
    else
        contentItem.innerText = content;
}

function getDisplayContent(message) {
    if (message.Type == 'response')
        return marked.parse(message.Content ?? '');

    if (message.Type == 'tool-calls')
        return `<pre>${formatJson(message.Content)}</pre>`;

    if (message.Type == 'tool-results')
        return `<pre>${formatToolResponses(message.Content)}</pre>`;

    return message.Content ?? '';
}

function formatToolResponses(content) {
    return JSON.stringify(JSON.parse(content ?? '{}'), null, 4).substring(0, 1000);
}

function formatJson(content) {
    return JSON.stringify(JSON.parse(content ?? '{}'), null, 4);
}

function updateTokenUsageItem(renderedMessage, message) {
    if (!renderedMessage.statsListGroup)
        return;

    if (message.TokenUsage == null) {
        if (renderedMessage.tokenUsageItem)
            renderedMessage.tokenUsageItem.remove();

        renderedMessage.tokenUsageItem = null;
        return;
    }

    if (!renderedMessage.tokenUsageItem) {
        renderedMessage.tokenUsageItem = makeTokenUsageItem(message);
        renderedMessage.statsListGroup.appendChild(renderedMessage.tokenUsageItem);
        return;
    }

    renderedMessage.tokenUsageItem.innerText = formatTokenUsage(message.TokenUsage);
}

function makeTokenUsageItem(message) {
    if (message.TokenUsage == null)
        return null;

    return makeListItem(formatTokenUsage(message.TokenUsage));
}

function formatTokenUsage(value) {
    return `${Number(value).toLocaleString()} tokens`;
}

function formatDate(value) {
    return value?.replace('T', ' ').split('.')[0] ?? '';
}

function makeListGroup(listItems = [], classes = []) {
    return makeElementWithClasses('ul', ['list-group', ...classes], listItems);
}

function makeListItem(text, classes = [], html = false) {
    var item = makeElementWithClasses('li', ['list-group-item', ...classes]);

    if (html)
        item.innerHTML = text;
    else
        item.innerText = text;

    return item;
}

function makeElementWithClasses(element, classes = [], nodes = []) {
    var item = document.createElement(element);
    item.classList.add(...classes);
    item.append(...nodes);
    return item;
}

function setCurrentAction(text) {
    currentActionSpan.innerText = text;
}

function setCurrentActionIdle() {
    currentActionSpan.innerText = 'Idle.';
}

function splitModelId(id) {
    if (id == null) {
        return {
            id: null,
            provider: null,
            model: null
        };
    }

    var indexOfGt = id.indexOf('>') + 1;
    var provider = id.substring(0, indexOfGt).replace('<', '').replace('>', '');
    var model = id.substring(indexOfGt);

    return {
        id: id,
        provider: provider,
        model: model
    };
}

function getODataItems(response) {
    if (Array.isArray(response))
        return response;

    return response?.value ?? [];
}

async function handleCreateConversationEvent() {
    var name = newConversationName.value;
    var description = newConversationDescription.value;

    if (!name) {
        window.alert('Conversation name is required.');
        return;
    }

    var conversation = await api.post('Conversation', {
        Name: name,
        Description: description
    });

    newConversationName.value = null;
    newConversationDescription.value = null;

    var modal = bootstrap.Modal.getInstance(document.getElementById('new-conversation-modal'));
    modal.hide();

    await loadConversation(conversation.Id);
    await loadConversations();
}

async function handleDeleteConversationEvent(id) {
    await api.delete(`Conversation/${id}`);
    await loadConversations();

    if (activeConversation?.Id == id) {
        activeConversation = null;
        clearMessages();
        chatTitle.innerText = 'JustAnAiAgent';
    }
}

async function handleDeleteProjectEvent(id) {
    await api.delete(`AgenticProject/${id}`);
    await loadProjects();

    activeProjectConversations = activeProjectConversations.filter(conversation => conversation.ProjectId != id);

    if (activeProject?.Id == id) {
        activeProject = null;
        activeConversation = null;
        setActiveMessagesSource(null);
        clearMessages();
        refreshSendModeOptions();
        setSendMode({ type: 'chat' });
        chatTitle.innerText = 'JustAnAiAgent';
    }
}

function initEventListeners() {
    sendMessageButton.addEventListener('click', sendMessage);

    messageInput.addEventListener('keydown', function (e) {
        if (e.ctrlKey && e.key == 'Enter') {
            e.preventDefault();
            sendMessage();
        }
    });

    conversationsList.addEventListener('click', function (e) {
        var target = e.target.closest('[data-action]');

        if (!target)
            return;

        var action = target.getAttribute('data-action');

        if (action == 'load-conversation')
            loadConversation(target.getAttribute('data-conversation-id'));

        if (action == 'delete-conversation')
            handleDeleteConversationEvent(target.getAttribute('data-conversation-id'));
    });

    projectsList?.addEventListener('click', function (e) {
        var target = e.target.closest('[data-action]');

        if (!target)
            return;

        var action = target.getAttribute('data-action');

        if (action == 'open-project')
            window.location.href = `/Projects?projectId=${target.getAttribute('data-project-id')}`;

        if (action == 'delete-project')
            handleDeleteProjectEvent(target.getAttribute('data-project-id'));
    });

    messagesBox.addEventListener('click', function (e) {
        var target = e.target.closest('[data-action="load-more-messages"]');

        if (target)
            loadMoreMessages();
    });

    loadMoreConversationsButton.addEventListener('click', () => loadConversations(false));
    loadMoreProjectsButton?.addEventListener('click', () => loadProjects(false));
    newConversationAddButton.addEventListener('click', handleCreateConversationEvent);
}

async function start() {
    var query = new URLSearchParams(window.location.search);
    var requestedConversationId = query.get('conversationId');
    var conversations = await loadConversations();
    await loadModels();

    if (requestedConversationId)
        await loadConversation(requestedConversationId);
    else if (conversations.length > 0)
        await loadConversation(conversations[0].Id);
    else {
        await loadProjects();
        refreshSendModeOptions();
    }

    initEventListeners();
}

start();
