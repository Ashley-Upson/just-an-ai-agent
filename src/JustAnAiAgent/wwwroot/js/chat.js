var chatTitle = document.getElementById('title');
var messagesBox = document.getElementById('messages-box');
var conversationsList = document.getElementById('conversations-list');
var messageInput = document.getElementById('message-input');
var currentActionSpan = document.getElementById('current-action-span');
var modelsSelect = document.getElementById('model-select');
var modelSelectButton = document.getElementById('model-select-button');
var selectedModel = document.getElementById('selected-model');
var sendMessageButton = document.getElementById('send-message-button');

var newConversationName = document.getElementById('new-conversation-name');
var newConversationDescription = document.getElementById('new-conversation-description');
var newConversationAddButton = document.getElementById('create-conversation');

var activeConversation = null;
var selectedModelId = null;
var renderedMessages = new Map();

var conversationTemplate = `<div class="ms-2 me-auto conversation" onclick="loadConversation('{ID}')">{NAME}</div>
<button class="badge text-bg-danger" name="delete-conversation" data-conversation-id="{ID}">X</button>`;

var projectTemplate = ``;

var userMessageTemplate = ``;

var systemMessageTemplate = ``;

var modelThoughtTemplate = ``;

var modeMessageTemplate = ``;

async function loadConversations() {
    setCurrentAction('Loading conversations...');

    var conversations = await api.get('Conversation?$orderBy=LastmessageSentAt desc');

    var conversationItems = [];

    for (var conversation of conversations) {
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
        conversationItems.push(listItem);
    }

    conversationsList.innerHTML = '';
    conversationsList.append(...conversationItems);

    setCurrentActionIdle();

    return conversations;
}

async function loadConversation(id) {
    setCurrentAction('Loading conversation...');

    var conversation = await api.get(`Conversation/${id}?$expand=Messages`);

    chatTitle.innerText = `JustAnAiAgent | ${conversation.Name}`;
    messagesBox.innerHTML = '';
    renderedMessages.clear();

    for (var message of conversation.Messages) {
        switch (message.Type) {
            case 'user':
                addMessageToMessagesBox(message, 'user');
                break;

            case 'thought':
                addMessageToMessagesBox(message, 'model-thought');
                break;

            case 'response':
                addMessageToMessagesBox(message, 'model-response');
                break;

            case 'tool-calls':
                addMessageToMessagesBox(message, 'tool-calls');
                break;

            case 'tool-results':
                addMessageToMessagesBox(message, 'tool-responses');
                break;
        }
    }

    if(conversation.Messages && conversation.Messages.length > 0)
        setActiveModel(conversation.Messages[conversation.Messages.length - 1]?.ModelId);

    setCurrentActionIdle();

    activeConversation = conversation;
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

async function sendMessage(e) {
    if (activeConversation == null) {
        window.alert('Select a conversation before sending a message.');
        return;
    }

    var message = messageInput.value.trim();

    addMessageToMessagesBox({
        Content: message,
        CreatedAt: new Date().toISOString(),
        ModelId: selectedModelId
    }, 'user');

    setCurrentAction('Waiting for model response...');

    messageInput.disabled = true;
    sendMessageButton.disabled = true;

    try {
        await api.postStream(`Chat/ConversationWithNewMessageStream/${activeConversation.Id}`, {
            ConversationId: activeConversation.Id,
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

function setActiveModel(id) {
    selectedModelId = id;

    var split = splitModelId(id);
    selectedModel.innerText = `${split.provider} | ${split.model}`;
}

function addMessageToMessagesBox(message, perspective, showStats = true) {
    var renderedMessage = null;

    if (!perspective)
        return;

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

    if (message.Id && renderedMessage)
        renderedMessages.set(message.Id, renderedMessage);

    messagesBox.scrollTo({
        top: messagesBox.scrollHeight,
        left: 0,
        behaviour: 'smooth'
    });
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

    if (renderedMessage.statusItem)
        renderedMessage.statusItem.innerText = getMessageStatus(message);

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
    var statusItem = makeListItem(getMessageStatus(message));
    
    var statsItem = makeElementWithClasses('li', [], [
        makeListGroup([
            receivedAtItem,
            makeListItem(modelIdSplit.provider),
            makeListItem(modelIdSplit.model),
            statusItem
        ], ['list-group-horizontal'])
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
        statusItem,
        contentItem,
        html: false
    };
}

function renderModelResponse(message, renderStats = true) {
    var receivedAtItem = null;

    if (renderStats) {
        var modelIdSplit = splitModelId(message.ModelId);
        receivedAtItem = makeListItem(formatDate(message.ResponseReceivedAt));

        var statsItem = makeElementWithClasses('li', [], [
            makeListGroup([
                receivedAtItem,
                makeListItem(modelIdSplit.provider),
                makeListItem(modelIdSplit.model)
            ], ['list-group-horizontal'])
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
    var json = JSON.parse(content ?? '{}');

    for (var i in json) {
        try {
            json[i] = JSON.parse(json[i]);
        } catch {
            json[i] = json[i];
        }
    }

    return JSON.stringify(json, null, 4).substring(0, 1000);
}

function formatJson(content) {
    return JSON.stringify(JSON.parse(content ?? '{}'), null, 4);
}

function getMessageStatus(message) {
    if (message.IsStillRunning)
        return 'Running...';

    if (message.IsComplete)
        return 'Complete';

    return 'Pending';
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

async function handleLoadConversationEvent(id) {
    await loadConversation(id);
}

async function handleCreateConversationEvent(e) {
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

function initEventListeners() {
    sendMessageButton.addEventListener('click', sendMessage);

    conversationsList.addEventListener('click', function (e) {
        var action = e.target.getAttribute('data-action');

        if (action == 'load-conversation')
            handleLoadConversationEvent(e.target.getAttribute('data-conversation-id'));

        if (action == 'delete-conversation')
            handleDeleteConversationEvent(e.target.getAttribute('data-conversation-id'));
    });

    newConversationAddButton.addEventListener('click', handleCreateConversationEvent);
}

async function start() {
    var conversations = await loadConversations();
    var models = await loadModels();

    if (conversations.length > 0)
        await loadConversation(conversations[0].Id);

    initEventListeners();
}

start();
