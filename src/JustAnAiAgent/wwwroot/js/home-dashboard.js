var startConversationCard = document.getElementById('start-conversation-card');
var startProjectCard = document.getElementById('start-project-card');
var latestConversationsList = document.getElementById('latest-conversations-list');
var latestProjectsList = document.getElementById('latest-projects-list');

async function loadLatestConversations() {
    var response = await api.get('Conversation?$orderBy=LastmessageSentAt desc,CreatedAt desc&$top=5');
    var conversations = getODataItems(response);

    latestConversationsList.innerHTML = '';

    if (conversations.length == 0) {
        latestConversationsList.appendChild(makeEmptyItem('No conversations yet'));
        return;
    }

    latestConversationsList.append(...conversations.map(conversation =>
        makeLatestItem(conversation.Name, `/Chat?conversationId=${conversation.Id}`)));
}

async function loadLatestProjects() {
    var response = await api.get('AgenticProject?$orderBy=UpdatedAt desc,CreatedAt desc&$top=5');
    var projects = getODataItems(response);

    latestProjectsList.innerHTML = '';

    if (projects.length == 0) {
        latestProjectsList.appendChild(makeEmptyItem('No projects yet'));
        return;
    }

    latestProjectsList.append(...projects.map(project =>
        makeLatestItem(project.Name, `/Projects?projectId=${project.Id}`)));
}

async function startNewConversation() {
    var conversation = await api.post('Conversation', {
        Name: 'New conversation',
        Description: ''
    });

    window.location.href = `/Chat?conversationId=${conversation.Id}`;
}

async function startNewProject() {
    var conversation = await api.post('Conversation', {
        Name: 'New project',
        Description: ''
    });

    var project = await api.post('AgenticProject', {
        Name: 'New project',
        Description: '',
        ConversationId: conversation.Id
    });

    conversation.ProjectId = project.Id;
    await api.put(`Conversation/${conversation.Id}`, conversation);

    window.location.href = `/Projects?projectId=${project.Id}`;
}

function makeLatestItem(label, href) {
    var item = document.createElement('li');
    item.classList.add('list-group-item', 'list-group-item-action');
    item.innerText = label;
    item.addEventListener('click', function (event) {
        event.stopPropagation();
        window.location.href = href;
    });

    return item;
}

function makeEmptyItem(label) {
    var item = document.createElement('li');
    item.classList.add('list-group-item', 'text-secondary');
    item.innerText = label;

    return item;
}

function getODataItems(response) {
    if (Array.isArray(response))
        return response;

    return response?.value ?? [];
}

function wireCardKeyboard(card, action) {
    card.addEventListener('click', action);
    card.addEventListener('keydown', function (event) {
        if (event.key == 'Enter' || event.key == ' ') {
            event.preventDefault();
            action();
        }
    });
}

async function start() {
    wireCardKeyboard(startConversationCard, startNewConversation);
    wireCardKeyboard(startProjectCard, startNewProject);

    await Promise.all([
        loadLatestConversations(),
        loadLatestProjects()
    ]);
}

start();
