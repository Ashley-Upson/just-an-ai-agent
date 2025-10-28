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

    for (var message of conversation.Messages) {
        addMessageToMessagesBox(message, 'user');

        if (message.ModelThought)
            addMessageToMessagesBox(message, 'model-thought');

        if (message.ModelResponse)
            addMessageToMessagesBox(message, 'model-response', message.ModelThought == null);

        if (message.ToolCalls)
            addMessageToMessagesBox(message, 'tool-calls');

        if (message.ToolResponses)
            addMessageToMessagesBox(message, 'tool-responses');
    }

    if(conversation.Messages && conversation.Messages.length > 0)
        setActiveModel(conversation.Messages[conversation.Messages.length - 1].ModelId);

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
        UserPrompt: message,
        CreatedAt: new Date().toISOString(),
        ModelId: selectedModelId
    }, 'user');

    setCurrentAction('Waiting for model response...');

    messageInput.disabled = true;
    sendMessageButton.disabled = true;

    var response = await api.post(`Chat/ConversationWithNewMessage/${activeConversation.Id}`, {
        ConversationId: activeConversation.Id,
        UserPrompt: message,
        ModelId: selectedModelId
    });

    if (response.ModelThought)
        addMessageToMessagesBox(response, 'model-thought');

    if (response.ModelResponse)
        addMessageToMessagesBox(response, 'model-response', response.ModelThought == null);

    if (message.ToolCalls)
        addMessageToMessagesBox(message, 'tool-calls');

    if (message.ToolResponses)
        addMessageToMessagesBox(message, 'tool-responses');

    messageInput.disabled = false;
    messageInput.value = '';
    sendMessageButton.disabled = false;

    setCurrentActionIdle();

    loadConversations();
}

function setActiveModel(id) {
    selectedModelId = id;

    var split = splitModelId(id);
    selectedModel.innerText = `${split.provider} | ${split.model}`;
}

function addMessageToMessagesBox(message, perspective, showStats = true) {
    if (perspective == 'user')
        renderUserMessage(message);

    if (perspective == 'model-thought')
        renderModelThought(message);

    if (perspective == 'model-response')
        renderModelResponse(message, showStats);

    if (perspective == 'tool-calls')
        renderToolCalls(message);

    if (perspective == 'tool-responses')
        renderToolResponses(message);

    messagesBox.scrollTo({
        top: messagesBox.scrollHeight,
        left: 0,
        behaviour: 'smooth'
    });
}

function renderUserMessage(message) {
    var statsItem = makeElementWithClasses('li', ['d-flex', 'justify-content-end'], [
        makeListGroup([
            makeListItem(message.CreatedAt.replace('T', ' ').split('.')[0])
        ], ['list-group-horizontal'])
    ]);

    var messageItem = makeElementWithClasses('li', ['d-flex', 'justify-content-end'], [
        makeListGroup([
            makeListItem(message.UserPrompt, ['list-group-item-info'])
        ])
    ]);

    messagesBox.append(statsItem, messageItem);
}

function renderModelThought(message) {
    var modelIdSplit = splitModelId(message.ModelId);
    
    var statsItem = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem(message.ResponseReceivedAt?.replace('T', ' ').split('.')[0]),
            makeListItem(modelIdSplit.provider),
            makeListItem(modelIdSplit.model),
            makeListItem('Thinking...')
        ], ['list-group-horizontal'])
    ]);

    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem(message.ModelThought, ['list-group-item-light'])
        ])
    ]);

    messagesBox.append(statsItem, messageItem);
}

function renderModelResponse(message, renderStats = true) {
    if (renderStats) {
        var modelIdSplit = splitModelId(message.ModelId);

        var statsItem = makeElementWithClasses('li', [], [
            makeListGroup([
                makeListItem(message.ResponseReceivedAt?.replace('T', ' ').split('.')[0]),
                makeListItem(modelIdSplit.provider),
                makeListItem(modelIdSplit.model)
            ], ['list-group-horizontal'])
        ]);

        messagesBox.appendChild(statsItem);
    }

    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem(marked.parse(message.ModelResponse), ['list-group-item-dark'], true)
        ])
    ]);

    messagesBox.appendChild(messageItem);
}

function renderToolCalls(message) {
    var titleBar = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem('Using MCP tools...')
        ], ['list-group-horizontal'])
    ]);

    var toolCalls = JSON.stringify(JSON.parse(message.ToolCalls), null, 4);

    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem(`<pre>${toolCalls}</pre>`, ['list-group-item-primary'], true)
        ])
    ]);

    messagesBox.append(titleBar, messageItem);
}

function renderToolResponses(message) {
    var titleBar = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem('Tool responses')
        ], ['list-group-horizontal'])
    ]);

    var json = JSON.parse(message.ToolResponses);

    for (var i in json)
        json[i] = JSON.parse(json[i]);

    var toolResponses = JSON.stringify(json, null, 4);

    var messageItem = makeElementWithClasses('li', [], [
        makeListGroup([
            makeListItem(`<pre>${toolResponses.substring(0, 1000)}</pre>`, ['list-group-item-success'])
        ])
    ]);

    messagesBox.append(titleBar, messageItem);
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