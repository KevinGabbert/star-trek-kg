function getTerminalSessionId() {
  return localStorage.getItem('sessionId') || '';
}

function setTerminalSessionId(id) {
  if (id) localStorage.setItem('sessionId', id);
}

function normalizeLines(lines) {
  if (!lines) return [];
  return lines.map(line => String(line || '').replace(/<\/?pre>/gi, ''));
}

function splitHeader(lines) {
  const cleaned = normalizeLines(lines);
  const nonEmpty = cleaned.filter(l => l.trim() !== '');
  if (nonEmpty.length && nonEmpty[0].trim() === 'Err:') {
    return { isError: true, lines: cleaned.filter(l => l.trim() !== 'Err:') };
  }
  return { isError: false, lines: cleaned };
}

async function fetchPrompt(sessionId) {
  const res = await fetch('/api/prompt', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId })
  });
  const data = await res.json();
  if (data && data.sessionId) setTerminalSessionId(data.sessionId);
  return data && data.prompt ? data.prompt : 'Terminal: ';
}

async function sendTerminalCommand(command) {
  const res = await fetch('/api/command', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId: getTerminalSessionId(), command })
  });
  const data = await res.json();
  if (data && data.sessionId) setTerminalSessionId(data.sessionId);
  return data && data.lines ? data.lines : [];
}

jQuery(function ($) {
  if (!$('#termWindow').length || !$.fn.terminalWindow) {
    return;
  }

  const greetings =
    'Star Trek KG\\n' +
    'A modern, C# rewrite of the original 1971 Star Trek game by Mike Mayfield, with additional features... :)\\n\\n' +
    'Type \"start\" to begin, or \"term menu\" for terminal commands\\n' +
    'This application is currently under construction.\\n';

  const termHost = $('#termWindow');

  const terminal = termHost.terminalWindow(async function (command, term) {
    if (!command) return;

    const lines = await sendTerminalCommand(command);
    const result = splitHeader(lines);

    if (result.isError) {
      result.lines.forEach(item => {
        if (item.trim() !== '') term.error(item);
      });
    } else {
      result.lines.forEach(item => {
        if (item.trim() !== '') {
          term.echo(item, { raw: true });
        }
      });
    }

    const prompt = await fetchPrompt(getTerminalSessionId());
    term.set_prompt(prompt);
  }, {
    prompt: 'Terminal: ',
    name: 'termWindow',
    height: $(window).height(),
    greetings: greetings
  });

  fetchPrompt(getTerminalSessionId()).then(prompt => terminal.set_prompt(prompt));
});
