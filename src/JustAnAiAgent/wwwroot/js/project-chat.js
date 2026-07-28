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
var projectTasksList = document.getElementById('project-tasks-list');
var loadMoreProjectTasksButton = document.getElementById('load-more-project-tasks');
var projectLeftSidebar = document.getElementById('project-left-sidebar');
var projectEmptyState = document.getElementById('project-empty-state');
var projectMessageInputBox = document.getElementById('project-message-input-box');
var messageSendInfoBox = document.getElementById('message-send-info-box');

var newConversationName = document.getElementById('new-conversation-name');
var newConversationDescription = document.getElementById('new-conversation-description');
var newConversationAddButton = document.getElementById('create-conversation');
var newProjectName = document.getElementById('new-project-name');
var newProjectDescription = document.getElementById('new-project-description');
var newProjectAddButton = document.getElementById('create-project');
var newProjectTaskTitle = document.getElementById('new-project-task-title');
var newProjectTaskDescription = document.getElementById('new-project-task-description');
var newProjectTaskContext = document.getElementById('new-project-task-context');
var newProjectTaskOrderMode = document.getElementById('new-project-task-order-mode');
var newProjectTaskOrderTarget = document.getElementById('new-project-task-order-target');
var newProjectTaskAddButton = document.getElementById('create-project-task');

var pageSize = 25;
var activeConversation = null;
var activeProject = null;
var activeProjectConversations = [];
var selectedModelId = null;
var selectedMode = { type: 'chat' };
var conversationsSkip = 0;
var projectsSkip = 0;
var projectTasksSkip = 0;
var hasMoreConversations = false;
var hasMoreProjects = false;
var hasMoreProjectTasks = false;
var loadedProjects = [];
var loadedProjectTasks = [];
var activeMessagesODataUrl = null;
var messageLimit = pageSize;
var renderedMessages = new Map();
var renderedMessageOrder = [];

async function loadConversations(reset = true) {
    if (!activeProject) {
        conversationsList.innerHTML = '';
        loadMoreConversationsButton.classList.add('d-none');
        return [];
    }

    setCurrentAction('Loading conversations...');

    if (reset) {
        conversationsSkip = 0;
        conversationsList.innerHTML = '';
    }

    var response = await api.get(`Conversation?$filter=ProjectId eq ${activeProject.Id}&$orderBy=LastmessageSentAt desc&$skip=${conversationsSkip}&$top=${pageSize + 1}`);
    var conversations = getODataItems(response);
    hasMoreConversations = conversations.length > pageSize;
    conversations = conversations.slice(0, pageSize);
    conversationsSkip += conversations.length;

    conversationsList.append(...conversations.map(renderConversationListItem));
    loadMoreConversationsButton.classList.toggle('d-none', !hasMoreConversations);

    setCurrentActionIdle();

    return conversations;
}

async function loadProjectTasks(reset = true) {
    if (!activeProject) {
        projectTasksList.innerHTML = '';
        loadMoreProjectTasksButton.classList.add('d-none');
        loadedProjectTasks = [];
        refreshProjectTaskOrderTargets();
        return [];
    }

    setCurrentAction('Loading project tasks...');

    if (reset) {
        projectTasksSkip = 0;
        projectTasksList.innerHTML = '';
        loadedProjectTasks = [];
    }

    var response = await api.get(`ProjectTask?$filter=ProjectId eq ${activeProject.Id}&$orderBy=Order asc,CreatedAt desc&$skip=${projectTasksSkip}&$top=${pageSize + 1}`);
    var tasks = getODataItems(response);
    hasMoreProjectTasks = tasks.length > pageSize;
    tasks = tasks.slice(0, pageSize);
    projectTasksSkip += tasks.length;
    loadedProjectTasks.push(...tasks);

    if (loadedProjectTasks.length == 0)
        projectTasksList.appendChild(makeListItem('No project tasks', ['text-secondary']));
    else
        projectTasksList.append(...tasks.map(renderProjectTaskListItem));

    refreshProjectTaskOrderTargets();
    loadMoreProjectTasksButton.classList.toggle('d-none', !hasMoreProjectTasks);

    setCurrentActionIdle();

    return tasks;
}

async function loadProjects(reset = true) {
    setCurrentAction('Loading projects...');

    if (reset) {
        projectsSkip = 0;
        if (projectsList)
            projectsList.innerHTML = '';
        loadedProjects = [];
    }

    var response = await api.get(`AgenticProject?$orderBy=UpdatedAt desc,CreatedAt desc&$skip=${projectsSkip}&$top=${pageSize + 1}`);
    var projects = getODataItems(response);
    hasMoreProjects = projects.length > pageSize;
    projects = projects.slice(0, pageSize);
    projectsSkip += projects.length;
    loadedProjects.push(...projects);

    if (projectsList)
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
    refreshSendModeOptions();
    defaultSendModeForActiveConversation();

    setCurrentActionIdle();
}

async function loadProject(id) {
    setCurrentAction('Loading project...');

    activeProject = await api.get(`AgenticProject/${id}`);
    showProjectWorkspace();
    activeProjectConversations = await loadConversations();
    await loadProjectTasks();

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

function showProjectWorkspace() {
    projectLeftSidebar.classList.remove('d-none');
    projectEmptyState.classList.add('d-none');
    messagesBox.classList.remove('d-none');
    projectMessageInputBox.classList.remove('d-none');
    messageSendInfoBox.classList.remove('d-none');
}

function showProjectEmptyState() {
    projectLeftSidebar.classList.add('d-none');
    projectEmptyState.classList.remove('d-none');
    messagesBox.classList.add('d-none');
    projectMessageInputBox.classList.add('d-none');
    messageSendInfoBox.classList.add('d-none');
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
    var listItem = makeElementWithClasses('li', ['list-group-item', 'd-flex', 'justify-content-between', 'align-items-start', 'project']);

    var link = makeElementWithClasses('div', ['ms-2', 'me-auto']);
    link.setAttribute('data-action', 'load-project');
    link.setAttribute('data-project-id', project.Id);
    link.innerText = project.Name;

    var deleteButton = makeElementWithClasses('button', ['badge', 'text-bg-danger']);
    deleteButton.setAttribute('data-action', 'delete-project');
    deleteButton.setAttribute('data-project-id', project.Id);
    deleteButton.innerText = 'X';

    listItem.append(link, deleteButton);

    return listItem;
}

function renderProjectTaskListItem(task) {
    var listItem = makeElementWithClasses('li', ['list-group-item', 'd-flex', 'justify-content-between', 'align-items-start']);
    listItem.setAttribute('data-project-task-id', task.Id);
    var title = task.Title ?? 'Untitled task';

    if (task.CompletedAt)
        title = `${title} (done)`;

    var titleItem = makeElementWithClasses('div', ['me-2', 'project-task-title']);
    titleItem.innerText = title;

    var actions = makeElementWithClasses('div', ['btn-group', 'btn-group-sm', 'project-task-actions']);

    if (!task.StartedAt && !task.CompletedAt)
        actions.appendChild(makeProjectTaskActionButton('Start', 'start-project-task', task.Id, ['btn-outline-secondary']));

    if (task.CompletedAt)
        actions.appendChild(makeProjectTaskActionButton('Reopen', 'reopen-project-task', task.Id, ['btn-outline-secondary']));
    else
        actions.appendChild(makeProjectTaskActionButton('Done', 'complete-project-task', task.Id, ['btn-outline-success']));

    actions.appendChild(makeProjectTaskActionButton('Delete', 'delete-project-task', task.Id, ['btn-outline-danger']));
    listItem.append(titleItem, actions);

    return listItem;
}

function makeProjectTaskActionButton(label, action, taskId, classes) {
    var button = makeElementWithClasses('button', ['btn', ...classes]);
    button.setAttribute('type', 'button');
    button.setAttribute('data-action', action);
    button.setAttribute('data-project-task-id', taskId);
    button.innerText = label;

    return button;
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
    if (activeProject && !activeConversation)
        return await createProjectConversation(activeProject);

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

        return await createProjectConversation(project);
    }

    return activeConversation;
}

function refreshSendModeOptions() {
    if (!sendModeMenu) {
        updateSelectedSendModeText();
        return;
    }

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

async function createProjectConversation(project) {
    var conversation = await api.post('Conversation', {
        Name: `${project.Name} agent`,
        Description: `Project agent conversation for ${project.Name}`,
        ProjectId: project.Id
    });

    activeConversation = conversation;
    activeProjectConversations.unshift(conversation);
    setActiveMessagesSource(`Message?$filter=Conversation/ProjectId eq ${project.Id}&$orderby=CreatedAt desc`);
    setSendMode({ type: 'project-agent', project, conversation });
    await loadConversations();

    return conversation;
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
    if (!activeProject) {
        window.alert('Select a project before creating a conversation.');
        return;
    }

    var name = newConversationName.value;
    var description = newConversationDescription.value;

    if (!name) {
        window.alert('Conversation name is required.');
        return;
    }

    var conversation = await api.post('Conversation', {
        Name: name,
        Description: description,
        ProjectId: activeProject.Id
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
    activeProjectConversations = await loadConversations();

    if (activeConversation?.Id == id) {
        activeConversation = activeProjectConversations[0] ?? null;
        await loadMessagesFromActiveSource();
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
        chatTitle.innerText = 'JustAnAiAgent | Projects';
        showProjectEmptyState();
    }
}

async function handleCreateProjectEvent() {
    var name = newProjectName.value;
    var description = newProjectDescription.value;

    if (!name) {
        window.alert('Project name is required.');
        return;
    }

    var conversation = await api.post('Conversation', {
        Name: name,
        Description: description
    });

    var project = await api.post('AgenticProject', {
        Name: name,
        Description: description,
        ConversationId: conversation.Id
    });

    conversation.ProjectId = project.Id;
    await api.put(`Conversation/${conversation.Id}`, conversation);

    newProjectName.value = null;
    newProjectDescription.value = null;

    var modal = bootstrap.Modal.getInstance(document.getElementById('new-project-modal'));
    modal.hide();

    await loadProjects();
    await loadProject(project.Id);
}

async function handleCreateProjectTaskEvent() {
    if (!activeProject) {
        window.alert('Select a project before creating a task.');
        return;
    }

    var title = newProjectTaskTitle.value.trim();

    if (!title) {
        window.alert('Task title is required.');
        return;
    }

    var newTask = {
        ProjectId: activeProject.Id,
        Order: calculateProjectTaskOrder(),
        Title: title,
        Description: newProjectTaskDescription.value ?? '',
        AdditionalContext: newProjectTaskContext.value || null
    };

    var createdTask = await api.post('ProjectTask', newTask);
    await normalizeProjectTaskOrder(createdTask);

    newProjectTaskTitle.value = null;
    newProjectTaskDescription.value = null;
    newProjectTaskContext.value = null;
    newProjectTaskOrderMode.value = 'end';

    var modal = bootstrap.Modal.getInstance(document.getElementById('new-project-task-modal'));
    modal.hide();

    await loadProjectTasks();
}

function calculateProjectTaskOrder() {
    if (loadedProjectTasks.length == 0)
        return 1000;

    var mode = newProjectTaskOrderMode.value;
    var targetTask = loadedProjectTasks.find(task => task.Id == newProjectTaskOrderTarget.value);
    var minOrder = Math.min(...loadedProjectTasks.map(task => task.Order ?? 0));
    var maxOrder = Math.max(...loadedProjectTasks.map(task => task.Order ?? 0));

    if (mode == 'beginning')
        return minOrder - 1000;

    if (mode == 'before' && targetTask)
        return (targetTask.Order ?? 0) - 1;

    if (mode == 'after' && targetTask)
        return (targetTask.Order ?? 0) + 1;

    return maxOrder + 1000;
}

async function normalizeProjectTaskOrder(createdTask) {
    var orderedTasks = [...loadedProjectTasks, createdTask]
        .sort((left, right) => (left.Order ?? 0) - (right.Order ?? 0) || (left.CreatedAt ?? '').localeCompare(right.CreatedAt ?? ''));

    for (var index = 0; index < orderedTasks.length; index++) {
        var task = orderedTasks[index];
        var nextOrder = (index + 1) * 1000;

        if (task.Order == nextOrder)
            continue;

        await api.put(`ProjectTask/${task.Id}`, {
            Id: task.Id,
            ProjectId: task.ProjectId,
            ConversationId: task.ConversationId,
            Order: nextOrder,
            Title: task.Title,
            Description: task.Description ?? '',
            AdditionalContext: task.AdditionalContext,
            CreatedAt: task.CreatedAt,
            StartedAt: task.StartedAt,
            CompletedAt: task.CompletedAt
        });
    }
}

async function handleDeleteProjectTaskEvent(id) {
    await api.delete(`ProjectTask/${id}`);
    await loadProjectTasks();
}

async function updateProjectTaskState(id, changes) {
    var task = loadedProjectTasks.find(task => task.Id == id) ?? await api.get(`ProjectTask/${id}`);

    await api.put(`ProjectTask/${id}`, {
        Id: task.Id,
        ProjectId: task.ProjectId,
        ConversationId: task.ConversationId,
        Order: task.Order,
        Title: task.Title,
        Description: task.Description ?? '',
        AdditionalContext: task.AdditionalContext,
        CreatedAt: task.CreatedAt,
        StartedAt: task.StartedAt,
        CompletedAt: task.CompletedAt,
        ...changes
    });

    await loadProjectTasks();
}

function refreshProjectTaskOrderTargets() {
    if (!newProjectTaskOrderTarget)
        return;

    newProjectTaskOrderTarget.innerHTML = '';

    if (loadedProjectTasks.length == 0) {
        var emptyOption = document.createElement('option');
        emptyOption.value = '';
        emptyOption.innerText = 'No tasks loaded';
        newProjectTaskOrderTarget.appendChild(emptyOption);
        newProjectTaskOrderTarget.disabled = true;
        return;
    }

    for (var task of loadedProjectTasks) {
        var option = document.createElement('option');
        option.value = task.Id;
        option.innerText = task.Title ?? 'Untitled task';
        newProjectTaskOrderTarget.appendChild(option);
    }

    newProjectTaskOrderTarget.disabled = false;
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

        if (action == 'load-project')
            loadProject(target.getAttribute('data-project-id'));

        if (action == 'delete-project')
            handleDeleteProjectEvent(target.getAttribute('data-project-id'));
    });

    projectTasksList.addEventListener('click', function (e) {
        var target = e.target.closest('[data-action]');

        if (!target)
            return;

        var taskId = target.getAttribute('data-project-task-id');
        var action = target.getAttribute('data-action');

        if (action == 'start-project-task')
            updateProjectTaskState(taskId, { StartedAt: new Date().toISOString() });

        if (action == 'complete-project-task')
            updateProjectTaskState(taskId, {
                StartedAt: new Date().toISOString(),
                CompletedAt: new Date().toISOString()
            });

        if (action == 'reopen-project-task')
            updateProjectTaskState(taskId, {
                StartedAt: null,
                CompletedAt: null
            });

        if (action == 'delete-project-task')
            handleDeleteProjectTaskEvent(taskId);
    });

    messagesBox.addEventListener('click', function (e) {
        var target = e.target.closest('[data-action="load-more-messages"]');

        if (target)
            loadMoreMessages();
    });

    loadMoreConversationsButton.addEventListener('click', () => loadConversations(false));
    loadMoreProjectsButton?.addEventListener('click', () => loadProjects(false));
    loadMoreProjectTasksButton.addEventListener('click', () => loadProjectTasks(false));
    newConversationAddButton.addEventListener('click', handleCreateConversationEvent);
    newProjectAddButton.addEventListener('click', handleCreateProjectEvent);
    newProjectTaskAddButton.addEventListener('click', handleCreateProjectTaskEvent);
}

async function start() {
    var query = new URLSearchParams(window.location.search);
    var requestedProjectId = query.get('projectId');

    showProjectEmptyState();
    await loadProjects();
    await loadModels();

    if (requestedProjectId)
        await loadProject(requestedProjectId);
    else
        refreshSendModeOptions();

    initEventListeners();
}

start();
