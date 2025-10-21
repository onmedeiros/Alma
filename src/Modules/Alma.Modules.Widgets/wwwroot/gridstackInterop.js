// Garante carregamento de CSS e do bundle UMD como script clássico
function ensureCssOnce() {
    if (!document.getElementById('gridstack-css')) {
        const link = document.createElement('link');
        link.id = 'gridstack-css';
        link.rel = 'stylesheet';
        // Caminho absoluto para Static Web Assets + cache-bust na primeira carga
        link.href = '/_content/Alma.Modules.Widgets/gridstack.min.css';
        document.head.appendChild(link);
    }

    if (!document.getElementById('gridstack-alma-css')) {
        const link = document.createElement('link');
        link.id = 'gridstack-css';
        link.rel = 'stylesheet';
        // Caminho absoluto para Static Web Assets + cache-bust na primeira carga
        link.href = '/_content/Alma.Modules.Widgets/gridstack.alma.css';
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

    const url = '/_content/Alma.Modules.Widgets/gridstack-all.js?v=' + encodeURIComponent('1');

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
    console.log('initializeGridStack called with options:', options, 'and interopReference:', interopReference);

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

    console.log(grid);

    // Setup drag in class
    GridStackGlobal.setupDragIn('.grid-stack-available-widget');

    // Setup event listeners
    grid.on('added',
        async (event, items) => {
            var args = getEventArgs(event, items);

            if (args.widgets.length === 0)
                return;

            // Before add in blazor, remove selection (items with attribute to-add='true') from grid-stack container
            var elementsToRemove = document.querySelectorAll('.grid-stack .grid-stack-item[to-add="true"]');

            elementsToRemove
                .forEach(el => grid.removeWidget(el));

            await interopReference.invokeMethodAsync('HandleAdded', getEventArgs(event, items));
        }
    );

    grid.on('change',
        async (event, items) => {
            items.forEach(item => console.log('Item changed:', item));
            await interopReference.invokeMethodAsync('HandleChanged', getEventArgs(event, items));
        }
    );

    grid.on('removed',
        async (event, items) => {
            await interopReference.invokeMethodAsync('HandleRemoved', getEventArgs(event, items));
        }
    );

    return grid;
}

function getEventArgs(event, items) {
    var widgets = [];

    if (event.type === 'added') {
        widgets = items.filter(item => item.el.attributes["to-add"] !== undefined && item.el.attributes["to-add"].value === "true");
    }
    else {
        widgets = items;
    }

    return {
        eventName: event.type,
        widgets: widgets.map(item => ({
            id: item.el.id,
            type: item.el.attributes["type"]?.value,
            w: item.w,
            h: item.h,
            x: item.x,
            y: item.y
        }))
    }
}

// https://github.com/decelis/BlazorGridStack/blob/main/BlazorGridStack/wwwroot/gridStackInterop.js
// https://github.com/decelis/BlazorGridStack/blob/main/BlazorGridStack/GridStackInterop.cs