var chatTitle = document.getElementById('title');
var messagesBox = document.getElementById('messages-box');
var conversationsList = document.getElementById('conversations-list');
var messageInput = document.getElementById('message-input');
var activeConversation = null;

var conversationTemplate = `<div class="ms-2 me-auto conversation" onclick="loadConversation('{ID}')">{NAME}</div>
<button class="badge text-bg-danger" name="delete-conversation" data-conversation-id="{ID}">X</button>`;

var projectTemplate = ``;

var userMessageTemplate = ``;

var systemMessageTemplate = ``;

var modelThoughtTemplate = ``;

var modeMessageTemplate = ``;

async function loadConversations() {
    var conversations = await api.get('Conversation');

    for (var conversation of conversations) {
        var element = document.createElement('li');
        element.classList.add('list-group-item', 'd-flex', 'justify-content-between', 'align-items-start');
        element.innerHTML = conversationTemplate
            .replaceAll('{ID}', conversation.Id)
            .replace('{NAME}', conversation.Name);
        conversationsList.appendChild(element);
    }
}

async function loadConversation(id) {
    var conversation = await api.get(`Conversation/${id}?$expand=Messages`);

    chatTitle.innerText = `JustAnAiAgent | ${conversation.Name}`;

    for (var message of conversation.Messages) {
        addMessageToMessagesBox(message.UserPrompt);

        if (message.ModelThought)
            addMessageToMessagesBox(message.ModelThought);

        if (message.ModelResponse)
            addMessageToMessagesBox(message.ModelResponse);
    }

    activeConversation = conversation;
}

async function sendMessage(e) {
    if (activeConversation == null) {
        window.alert('Select a conversation before sending a message.');
        return;
    }

    var message = messageInput.value.trim();

    addMessageToMessagesBox(message);

    var response = await api.post(`Chat/ConversationWithNewMessage/${activeConversation.Id}`, {
        ConversationId: activeConversation.Id,
        UserPrompt: message,
        ModelId: 'gpt-oss:20b'
    });

    if (response.ModelThought)
        addMessageToMessagesBox(response.ModelThought);

    if (response.ModelResponse)
        addMessageToMessagesBox(response.ModelResponse);
}

function addMessageToMessagesBox(message) {
    var item = document.createElement('li');
    item.classList.add('list-group-item');
    item.innerText = message;
    messagesBox.appendChild(item);
    messagesBox.scrollTo({
        top: messagesBox.scrollHeight,
        left: 0,
        behaviour: 'smooth'
    });
}

loadConversations();