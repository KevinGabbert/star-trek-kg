const output = document.getElementById('output');
const commandInput = document.getElementById('command');
const sendBtn = document.getElementById('send');
const startBtn = document.getElementById('start');
const clearBtn = document.getElementById('clear');
const modeSelect = document.getElementById('mode');
const terminalSection = document.getElementById('terminal-mode');
const commandSection = document.getElementById('command-mode');

function stripPreTags(text) {
  return text.replace(/<\/?pre>/gi, '');
}

function appendLines(lines) {
  if (!lines || !output) return;
  const cleaned = lines.map(line => stripPreTags(String(line)));
  output.textContent += cleaned.join('\n') + '\n';
  output.scrollTop = output.scrollHeight;
}

function getSessionId() {
  return localStorage.getItem('sessionId') || '';
}

function setSessionId(id) {
  if (id) localStorage.setItem('sessionId', id);
}

function setMode(mode) {
  const selected = mode === 'command' ? 'command' : 'terminal';
  if (terminalSection) terminalSection.classList.toggle('active', selected === 'terminal');
  if (commandSection) commandSection.classList.toggle('active', selected === 'command');
  if (modeSelect) modeSelect.value = selected;
  localStorage.setItem('uiMode', selected);
}

if (modeSelect) {
  const savedMode = localStorage.getItem('uiMode') || 'terminal';
  setMode(savedMode);
  modeSelect.addEventListener('change', () => setMode(modeSelect.value));
}

async function startGame() {
  const sessionId = getSessionId();
  const res = await fetch('/api/start', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId })
  });
  const data = await res.json();
  setSessionId(data.sessionId);
  appendLines(data.lines);
}

async function sendCommand() {
  if (!commandInput) return;
  const cmd = commandInput.value.trim();
  if (!cmd) return;
  const res = await fetch('/api/command', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId: getSessionId(), command: cmd })
  });
  const data = await res.json();
  setSessionId(data.sessionId);
  appendLines(data.lines);
  commandInput.value = '';
}

if (sendBtn) {
  sendBtn.addEventListener('click', sendCommand);
}
if (startBtn) {
  startBtn.addEventListener('click', startGame);
}
if (clearBtn) {
  clearBtn.addEventListener('click', () => { if (output) output.textContent = ''; });
}
if (commandInput) {
  commandInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') sendCommand();
  });
}
