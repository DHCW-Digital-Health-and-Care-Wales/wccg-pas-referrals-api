let editor = CodeMirror.fromTextArea(document.getElementById('code'), {
  lineNumbers: true,
  mode: "application/json",
  theme: "neat",
  matchBrackets: true,
  autoCloseBrackets: true,
  indentWithTabs: true,
  smartIndent: true
});
