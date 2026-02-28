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

function randomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function toHex(value) {
  const hex = value.toString(16).padStart(2, '0');
  return hex.toUpperCase();
}

function randomNebulaColor() {
  const roll = Math.random();
  if (roll < 0.03) {
    const r = randomInt(200, 255);
    const g = randomInt(180, 255);
    const b = randomInt(0, 40);
    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
  }

  if (roll < 0.515) {
    const r = randomInt(180, 255);
    const g = randomInt(0, 50);
    const b = randomInt(0, 60);
    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
  }

  const r = randomInt(0, 60);
  const g = randomInt(0, 80);
  const b = randomInt(180, 255);
  return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
}

const NEBULA_COLOR_WIDTH = 24;

function shouldColorizeNebulaLine(line, hasNebulaContext) {
  if (!hasNebulaContext) return false;

  const plusMinusCount = (line.match(/[+-]/g) || []).length;
  if (plusMinusCount < 1) return false;

  const lower = line.toLowerCase();
  if (lower.includes("star trek") || lower.includes("mission:") || lower.includes("ncc") || lower.includes("type '") || lower.includes("game started")) {
    return false;
  }

  return true;
}

function colorizeNebulaLine(line) {
  let formatted = '';
  let isFormatted = false;

  for (let i = 0; i < line.length; i++) {
    const ch = line[i];
    if ((ch === '+' || ch === '-') && i < NEBULA_COLOR_WIDTH) {
      const color = randomNebulaColor();
      formatted += `[[;${color};]${ch}]`;
      isFormatted = true;
    } else {
      formatted += ch;
    }
  }

  return { text: formatted, formatted: isFormatted };
}

function colorizeConditionBorder(line, condition) {
  if (!condition) return { text: line, formatted: false };
  if (!line || line.indexOf('----') === -1) return { text: line, formatted: false };
  if (!(line.includes('╙') || line.includes('╚') || line.includes('╘') || line.includes('╜') || line.includes('└') || line.includes('┘') || line.includes('┴') || line.includes('╝') || line.includes('═'))) {
    return { text: line, formatted: false };
  }
  const color = condition === 'red'
    ? '#ff3b30'
    : (condition === 'blue' ? '#4da3ff' : '#00ff00');
  const colored = line.replace(/----/g, `[[;${color};]----]`);
  return { text: colored, formatted: true };
}

function splitHeader(lines) {
  const cleaned = normalizeLines(lines);
  let headerIndex = -1;
  for (let i = 0; i < cleaned.length; i++) {
    if (cleaned[i].trim() !== '') {
      headerIndex = i;
      break;
    }
  }

  if (headerIndex >= 0 && cleaned[headerIndex].trim() === 'Err:') {
    const withoutHeader = cleaned.slice(0, headerIndex).concat(cleaned.slice(headerIndex + 1));
    return { isError: true, lines: withoutHeader };
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

async function fetchClientSettings() {
  try {
    const res = await fetch('/api/settings', { method: 'GET' });
    const data = await res.json();
    const autoStartMode = data && typeof data.autoStartMode === 'string'
      ? data.autoStartMode.toLowerCase()
      : 'game';
    return { autoStart: !!(data && data.autoStart), autoStartMode };
  } catch {
    return { autoStart: false, autoStartMode: 'game' };
  }
}

jQuery(function ($) {
  if (!$('#termWindow').length || !$.fn.terminalWindow) {
    return;
  }

  const termHost = $('#termWindow');
  let lastCondition = null;
  let lastInNebula = false;

  async function processCommand(command, term) {
    if (!command) return;
    const lines = await sendTerminalCommand(command);
    const result = splitHeader(lines);

    const hasNebulaContext = result.lines.some(line => line.toLowerCase().includes("nebula"));
    const hasConditionLine = result.lines.find(line => line.includes("Condition: ") || line.includes("Condition RED"));
    if (hasConditionLine) {
      if (hasConditionLine.includes("Condition: RED") || hasConditionLine.includes("Condition RED")) {
        lastCondition = 'red';
      } else if (hasConditionLine.includes("Condition: GREEN")) {
        lastCondition = 'green';
      }
    } else if (!lastCondition) {
      if (result.lines.some(line => line.includes("Warning: Shields are down."))) {
        lastCondition = 'green';
      }
    }
    const nebulaLine = result.lines.find(line => line.toLowerCase().includes("sector:") && line.toLowerCase().includes("nebula"));
    if (nebulaLine || result.lines.some(line => line.toLowerCase().includes("while in nebula"))) {
      lastInNebula = true;
    } else if (hasNebulaContext) {
      lastInNebula = true;
    } else {
      lastInNebula = false;
    }

    if (result.isError) {
      result.lines.forEach(item => {
        term.error(item);
      });
    } else {
      let inLrsBlock = false;
      result.lines.forEach(item => {
        if (item.includes('*** Long Range Scan ***')) {
          inLrsBlock = true;
        } else if (item.trim().startsWith('NCC ')) {
          inLrsBlock = false;
        }
        const borderCondition = lastInNebula ? 'blue' : lastCondition;
        const coloredBorder = colorizeConditionBorder(item, borderCondition);
        if (coloredBorder.formatted) {
          term.echo(coloredBorder.text, { raw: false });
          return;
        }
        if (!inLrsBlock && shouldColorizeNebulaLine(item, hasNebulaContext)) {
          const colored = colorizeNebulaLine(item);
          term.echo(colored.text, { raw: false });
        } else {
          term.echo(item, { raw: true });
        }
      });
    }

    const prompt = await fetchPrompt(getTerminalSessionId());
    term.set_prompt(prompt);
  }

  (async function initializeTerminal() {
    const settings = await fetchClientSettings();
    const greetings =
      'Star Trek KG\n\n' +
      'A modern, C# rewrite of the original 1971 Star Trek game by Mike Mayfield, with additional features... :)\n\n' +
      (settings.autoStart ? '' : 'Type "start" or "war games" to begin, or "term menu" for terminal commands\n') +
      'This application is currently under construction.\n';

    const terminalWindow = termHost.terminalWindow(async function (command, term) {
      await processCommand(command, term);
    }, {
      prompt: 'Terminal: ',
      name: 'termWindow',
      height: $(window).height(),
      greetings: greetings
    });
    const terminal = terminalWindow.terminal || terminalWindow;

    const prompt = await fetchPrompt(getTerminalSessionId());
    terminal.set_prompt(prompt);

    if (settings.autoStart && prompt.trim() === 'Terminal:') {
      const startCommand = settings.autoStartMode === 'war games' ? 'war games' : 'start';
      await processCommand(startCommand, terminal);
    }
  })();
});
