// Garante carregamento de CSS e do bundle UMD como script clássico
function ensureCssOnce() {
    if (!document.getElementById('gridstack-css')) {
        const link = document.createElement('link');
        link.id = 'gridstack-css';
        link.rel = 'stylesheet';
        // Caminho absoluto para Static Web Assets + cache-bust na primeira carga
        link.href = '/_content/Alma.Modules.Dashboards/gridstack.min.css?v=' + encodeURIComponent('1');
        document.head.appendChild(link);
    }
}

function injectScriptTag(url) {
    return new Promise((resolve, reject) => {
        const existing = document.getElementById('gridstack-lib');
        if (existing) {
            existing.addEventListener('load', () => resolve(), { once: true });
            existing.addEventListener('error', (e) => reject(e), { once: true });
            return;
        }
        const script = document.createElement('script');
        script.id = 'gridstack-lib';
        script.src = url;
        script.async = true;
        script.onload = () => resolve();
        script.onerror = (e) => reject(e);
        document.head.appendChild(script);
    });
}

async function evalScriptForceGlobal(url) {
    const res = await fetch(url, { cache: 'no-cache' });
    if (!res.ok) throw new Error('Failed to fetch GridStack script: ' + res.status);
    const code = await res.text();
    // Força caminho global do UMD: exports/module/define indefinidos
    const wrapped = `(()=>{ var module = undefined; var exports = undefined; var define = undefined; var self = window;\n${code}\n})();`;
    // eslint-disable-next-line no-eval
    eval(wrapped);
}

async function ensureGridStackOnce() {
    if (globalThis.GridStack) return;

    const url = '/_content/Alma.Modules.Dashboards/gridstack-all.js?v=' + encodeURIComponent('1');

    // 1) Tenta via <script>
    // Desabilita AMD temporariamente (caso exista) para preferir caminho global
    const hadDefine = typeof globalThis.define === 'function' && globalThis.define?.amd;
    const defineBackup = hadDefine ? globalThis.define : undefined;
    if (hadDefine) {
        try { globalThis.define = undefined; } catch { try { delete globalThis.define; } catch { } }
    }
    try {
        await injectScriptTag(url);
    } finally {
        if (hadDefine) globalThis.define = defineBackup;
    }

    if (!globalThis.GridStack && globalThis.exports?.GridStack) {
        globalThis.GridStack = globalThis.exports.GridStack;
    }

    if (globalThis.GridStack) return;

    // 2) Fallback: fetch + eval em sandbox que força global
    await evalScriptForceGlobal(url);
}

export async function initializeGridStack(options, interopReference) {
    ensureCssOnce();

    await ensureGridStackOnce();

    const GridStackGlobal = globalThis.GridStack;
    if (!GridStackGlobal) {
        console.error('GridStack bundle carregado mas GridStack não encontrado no globalThis.', {
            typeofDefine: typeof globalThis.define,
            hasExports: !!globalThis.exports,
            exportsKeys: globalThis.exports ? Object.keys(globalThis.exports) : []
        });
        throw new Error('GridStack library failed to load.');
    }

    var grid = GridStackGlobal.init(options || {});
}

// https://github.com/decelis/BlazorGridStack/blob/main/BlazorGridStack/wwwroot/gridStackInterop.js
// https://github.com/decelis/BlazorGridStack/blob/main/BlazorGridStack/GridStackInterop.cs