// Read-only: extract a Nexus mod's current description BBCode + version + title
// from its edit page. Run it in the browser console of a signed-in session.
// No Playwright, no automation profile.
//
// Page: https://www.nexusmods.com/games/stardewvalley/mods/<modId>/edit/general
// Wait for .sceditor-container to exist before running.
//
// Returns { title, version, descLen, desc } as a JSON string.
(() => {
  const tas = [...document.querySelectorAll('textarea')]
    .filter(t => /describe your mod/i.test(t.placeholder || ''));
  const ta = tas.sort((a, b) => (b.value || '').length - (a.value || '').length)[0];
  const ver = document.querySelector('#mod-version');
  const title = document.querySelector('#mod-name, input[name="name"]');
  const desc = ta ? ta.value : '';
  return JSON.stringify({
    title: title ? title.value : null,
    version: ver ? ver.value : null,
    descLen: desc.length,
    desc,
  });
})();
