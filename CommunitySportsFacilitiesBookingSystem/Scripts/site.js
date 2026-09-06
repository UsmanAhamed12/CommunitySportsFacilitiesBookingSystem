(function () {
    'use strict';
    document.querySelectorAll('table').forEach(function (table) {
        if (!table.parentElement.classList.contains('table-responsive')) {
            var wrap = document.createElement('div'); wrap.className = 'table-responsive';
            table.parentNode.insertBefore(wrap, table); wrap.appendChild(table);
        }
    });
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (event.defaultPrevented || !form.checkValidity()) return;
            if (window.jQuery && jQuery.fn.valid && !jQuery(form).valid()) return;
            if (form.dataset.submitting === 'true') { event.preventDefault(); return; }
            if ((form.method || '').toLowerCase() !== 'post') return;
            form.dataset.submitting = 'true';
            var button = form.querySelector('button[type="submit"],input[type="submit"]');
            if (button) { button.setAttribute('aria-busy', 'true'); button.classList.add('disabled'); }
        });
    });
    var estimate = document.getElementById('booking-estimate');
    if (estimate) {
        function update() {
            var start = document.getElementById('StartTime').value.split(':');
            var end = document.getElementById('EndTime').value.split(':');
            var minutes = (+end[0] * 60 + +end[1]) - (+start[0] * 60 + +start[1]);
            estimate.textContent = minutes > 0 ? 'LKR ' + (minutes / 60 * +estimate.dataset.rate).toLocaleString('en-LK', {minimumFractionDigits:2,maximumFractionDigits:2}) : 'Choose a valid time';
        }
        document.getElementById('StartTime').addEventListener('input', update);
        document.getElementById('EndTime').addEventListener('input', update); update();
    }
}());
