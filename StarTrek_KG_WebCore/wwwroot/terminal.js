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
    const terminalMenuColor = data && typeof data.terminalMenuColor === 'string' && data.terminalMenuColor.trim()
      ? data.terminalMenuColor.trim()
      : '#4da3ff';
    const deuteriumColor = data && typeof data.deuteriumColor === 'string' && data.deuteriumColor.trim()
      ? data.deuteriumColor.trim()
      : '#00ff00';
    const hostileColor = data && typeof data.hostileColor === 'string' && data.hostileColor.trim()
      ? data.hostileColor.trim()
      : '#ff0000';
    const starbaseColor = data && typeof data.starbaseColor === 'string' && data.starbaseColor.trim()
      ? data.starbaseColor.trim()
      : '#ffff00';
    const starColor = data && typeof data.starColor === 'string' && data.starColor.trim()
      ? data.starColor.trim()
      : '#ffffff';
    const galacticBarrierColor = data && typeof data.galacticBarrierColor === 'string' && data.galacticBarrierColor.trim()
      ? data.galacticBarrierColor.trim()
      : '#ffffff';
    return { autoStart: !!(data && data.autoStart), autoStartMode, terminalMenuColor, deuteriumColor, hostileColor, starbaseColor, starColor, galacticBarrierColor };
  } catch {
    return {
      autoStart: false,
      autoStartMode: 'game',
      terminalMenuColor: '#4da3ff',
      deuteriumColor: '#00ff00',
      hostileColor: '#ff0000',
      starbaseColor: '#ffff00',
      starColor: '#ffffff',
      galacticBarrierColor: '#ffffff'
    };
  }
}

jQuery(function ($) {
  if (!$('#termWindow').length || !$.fn.terminalWindow) {
    return;
  }

  const termHost = $('#termWindow');
  let lastCondition = null;
  let lastInNebula = false;
  let clientSettings = {
    autoStart: false,
    autoStartMode: 'game',
    terminalMenuColor: '#4da3ff',
    deuteriumColor: '#00ff00',
    hostileColor: '#ff0000',
    starbaseColor: '#ffff00',
    starColor: '#ffffff',
    galacticBarrierColor: '#ffffff'
  };

  function isTerminalMenuHeader(line) {
    return line && line.trim() === '--- Terminal Menu ---';
  }

  function formatMenuLine(line) {
    return `[[;${clientSettings.terminalMenuColor};]${line}]`;
  }

  function clampColorChannel(value) {
    return Math.max(0, Math.min(255, value));
  }

  function parseHexColor(hex) {
    const normalized = String(hex || '').trim().replace(/^#/, '');
    if (!/^[0-9a-fA-F]{6}$/.test(normalized)) {
      return { r: 0, g: 255, b: 0 };
    }

    return {
      r: parseInt(normalized.slice(0, 2), 16),
      g: parseInt(normalized.slice(2, 4), 16),
      b: parseInt(normalized.slice(4, 6), 16)
    };
  }

  function shadeDeuteriumColor() {
    const base = parseHexColor(clientSettings.deuteriumColor);
    const delta = randomInt(-45, 45);
    const r = clampColorChannel(base.r + delta);
    const g = clampColorChannel(base.g + delta);
    const b = clampColorChannel(base.b + delta);
    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
  }

  function colorizeDeuteriumGlyphs(line) {
    if (!line || line.indexOf('.') < 0) {
      return { text: line, formatted: false };
    }

    const boundaries = '[\\s\\u2500-\\u257F]';
    const pattern = new RegExp(`(^|${boundaries})\\.(?=$|${boundaries})`, 'g');
    let formatted = false;
    const replaced = line.replace(pattern, (match, prefix) => {
      formatted = true;
      return `${prefix}[[;${shadeDeuteriumColor()};].]`;
    });

    return { text: replaced, formatted };
  }

  function reverseColor(text, color) {
    return `[[;#000000;${color}]${text}]`;
  }

  function colorizeImmediateScanEntities(line) {
    if (!line) {
      return { text: line, formatted: false };
    }

    let formatted = false;
    let updated = line;

    const applyWord = (target, color) => {
      const regex = new RegExp(`\\b${target}\\b`, 'g');
      updated = updated.replace(regex, (match) => {
        formatted = true;
        return reverseColor(match, color);
      });
    };

    applyWord('Deuterium Cloud', clientSettings.deuteriumColor);
    applyWord('Deuterium', clientSettings.deuteriumColor);
    applyWord('Starbase', clientSettings.starbaseColor);
    applyWord('Galactic Barrier', clientSettings.galacticBarrierColor);

    // Hostile ship names (default Klingon style name in IRS).
    updated = updated.replace(/\bIKC [A-Za-z'’`\-]+/g, (match) => {
      formatted = true;
      return reverseColor(match, clientSettings.hostileColor);
    });

    // Star names are currently rendered as uppercase names in IRS cells.
    updated = updated.replace(/\b[A-Z]{3,}(?: [A-Z]{3,})+\b/g, (match) => {
      // Skip already-special system labels.
      if (match === 'EMPTY SPACE' || match === 'GALACTIC BARRIER') {
        return match;
      }

      formatted = true;
      return reverseColor(match, clientSettings.starColor);
    });

    return { text: updated, formatted };
  }

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
      let inImmediateScanBlock = false;
      let inTerminalMenu = false;
      result.lines.forEach(item => {
        if (item.includes('*** Long Range Scan ***')) {
          inLrsBlock = true;
        } else if (item.trim().startsWith('NCC ')) {
          inLrsBlock = false;
        }

        if (item.includes('Immediate Range Scan')) {
          inImmediateScanBlock = true;
        } else if (inImmediateScanBlock && item.trim().startsWith('NCC ')) {
          inImmediateScanBlock = false;
        }

        if (isTerminalMenuHeader(item)) {
          inTerminalMenu = true;
        } else if (inTerminalMenu && item.trim() === '') {
          inTerminalMenu = false;
        }

        if (inTerminalMenu && item.trim() !== '') {
          term.echo(formatMenuLine(item), { raw: false });
          return;
        }

        const immediateScanStyled = colorizeImmediateScanEntities(item);
        if (immediateScanStyled.formatted) {
          term.echo(immediateScanStyled.text, { raw: false });
          return;
        }

        const borderCondition = lastInNebula ? 'blue' : lastCondition;
        const coloredBorder = colorizeConditionBorder(item, borderCondition);
        if (coloredBorder.formatted) {
          term.echo(coloredBorder.text, { raw: false });
          return;
        }

        const deuteriumColored = colorizeDeuteriumGlyphs(item);
        if (deuteriumColored.formatted) {
          term.echo(deuteriumColored.text, { raw: false });
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
    clientSettings = await fetchClientSettings();
    const terminalMenuText =
      formatMenuLine(' --- Terminal Menu ---') + '\n' +
      formatMenuLine('start - starts a normal game session') + '\n' +
      formatMenuLine('war games - starts a deterministic scenario session') + '\n' +
      formatMenuLine('systems cascade - starts systems failure survival mode') + '\n' +
      formatMenuLine('stop | exit | quit - ends the currently running game') + '\n' +
      formatMenuLine('clear session - clears the active session id') + '\n' +
      formatMenuLine('term menu - show terminal commands') + '\n' +
      formatMenuLine('release notes - see the latest release notes') + '\n' +
      formatMenuLine('clear - clear the screen') + '\n';

    const greetings =
      'Star Trek KG\n\n' +
      'A modern, C# rewrite of the original 1971 Star Trek game by Mike Mayfield, with additional features... :)\n\n' +
      (clientSettings.autoStart ? '' : terminalMenuText + '\n') +
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

    if (clientSettings.autoStart && prompt.trim() === 'Terminal:') {
      const startCommand =
        clientSettings.autoStartMode === 'war games'
          ? 'war games'
          : (clientSettings.autoStartMode === 'systems cascade' ? 'systems cascade' : 'start');
      await processCommand(startCommand, terminal);
    }
  })();
});
