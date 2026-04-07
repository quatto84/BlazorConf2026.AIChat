export function init() {
    const textArea = document.getElementById('text20');
    textArea.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const btn = document.getElementById('submitText20');
            btn.focus();
            btn.click();
        }
    });
}