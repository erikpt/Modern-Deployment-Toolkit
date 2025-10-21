const api = {
  list: '/api/editor/tasks',
  get: (id) => `/api/editor/tasks/${id}`,
  create: '/api/editor/tasks',
  update: (id) => `/api/editor/tasks/${id}`,
  delete: (id) => `/api/editor/tasks/${id}`,
  validate: '/api/editor/validate'
};

let currentId = null;

async function refreshList() {
  const res = await fetch(api.list);
  const tasks = await res.json();
  const container = document.getElementById('taskList');
  container.innerHTML = '';
  tasks.forEach(t => {
    const el = document.createElement('div');
    el.className = 'task-item';
    el.textContent = `${t.name} \u2014 ${new Date(t.modifiedDate).toLocaleString()}`;
    el.onclick = () => loadTask(t.id);
    container.appendChild(el);
  });
}

async function loadTask(id) {
  const res = await fetch(api.get(id));
  if (!res.ok) { alert('Failed to load'); return; }
  const t = await res.json();
  currentId = t.id;
  document.getElementById('taskName').value = t.name;
  document.getElementById('taskDesc').value = t.description;
  document.getElementById('editor').value = t.content;
  setStatus('Loaded');
}

async function newTask() {
  currentId = null;
  document.getElementById('taskName').value = '';
  document.getElementById('taskDesc').value = '';
  document.getElementById('editor').value = '<!-- Paste XML / JSON / YAML here -->';
  setStatus('New');
}

async function saveTask() {
  const name = document.getElementById('taskName').value;
  const desc = document.getElementById('taskDesc').value;
  const content = document.getElementById('editor').value;
  if (!name) { alert('Name is required'); return; }

  if (currentId) {
    const res = await fetch(api.update(currentId), {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, description: desc, content })
    });
    if (res.ok) { setStatus('Saved'); await refreshList(); }
    else { const err = await res.json(); setStatus('Save failed: ' + (err.error || JSON.stringify(err))); }
  } else {
    const res = await fetch(api.create, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, description: desc, content })
    });
    if (res.ok) { const data = await res.json(); currentId = data.id; setStatus('Created'); await refreshList(); }
    else { const err = await res.json(); setStatus('Create failed: ' + (err.error || JSON.stringify(err))); }
  }
}

async function deleteTask() {
  if (!currentId) { alert('No task selected'); return; }
  if (!confirm('Delete this task sequence?')) return;
  const res = await fetch(api.delete(currentId), { method: 'DELETE' });
  if (res.ok) { newTask(); await refreshList(); setStatus('Deleted'); }
  else { setStatus('Delete failed'); }
}

async function validateTask() {
  const content = document.getElementById('editor').value;
  const res = await fetch(api.validate, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ content }) });
  const body = await res.json();
  if (res.ok) { setStatus('Valid'); alert('Valid task sequence'); }
  else { setStatus('Invalid'); alert('Validation errors:\n' + (body.Errors ? body.Errors.join('\n') : JSON.stringify(body))); }
}

function setStatus(s) { document.getElementById('status').textContent = s; }

window.addEventListener('load', () => {
  document.getElementById('refreshBtn').onclick = refreshList;
  document.getElementById('newBtn').onclick = newTask;
  document.getElementById('saveBtn').onclick = saveTask;
  document.getElementById('deleteBtn').onclick = deleteTask;
  document.getElementById('validateBtn').onclick = validateTask;
  newTask();
  refreshList();
});