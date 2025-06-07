// Custom JavaScript برای بهبود SwaggerUI
window.addEventListener('load', function () {
    console.log('SwaggerUI Custom Script Loaded');

    // تابع برای بررسی و نمایش dropdown
    function setupDropdown() {
        setTimeout(function () {
            const selectElement = document.querySelector('.download-url-wrapper select');

            if (selectElement) {
                console.log('Dropdown found:', selectElement);

                // بهبود استایل dropdown
                selectElement.style.minWidth = '250px';
                selectElement.style.fontSize = '14px';
                selectElement.style.padding = '5px';
                selectElement.style.border = '1px solid #ccc';
                selectElement.style.borderRadius = '4px';

                // نمایش تعداد options
                console.log('Available definitions:', selectElement.options.length);

                // بهبود نمایش definitions
                const options = selectElement.querySelectorAll('option');
                options.forEach((option, index) => {
                    console.log(`Option ${index}:`, option.value, option.textContent);

                    if (option.value.includes('user')) {
                        option.textContent = '👤 User Management Service';
                    } else if (option.value.includes('wallet')) {
                        option.textContent = '💳 Wallet Payment Service';
                    } else if (option.value.includes('order')) {
                        option.textContent = '📦 Order Management Service';
                    } else if (option.value.includes('gateway') || option.value === '/swagger/v1/swagger.json') {
                        option.textContent = '🌐 API Gateway';
                    }
                });

                // اضافه کردن event listener برای تغییر definition
                selectElement.addEventListener('change', function () {
                    console.log('Selected definition changed to:', this.value);
                });

                // اگر dropdown خالی است، پیام نمایش بده
                if (selectElement.options.length <= 1) {
                    console.warn('Only one or no definitions found!');
                }

            } else {
                console.error('Dropdown not found!');
                // تلاش مجدد بعد از 2 ثانیه
                setTimeout(setupDropdown, 2000);
            }
        }, 1000);
    }

    // شروع تنظیم dropdown
    setupDropdown();

    // اضافه کردن helper text
    setTimeout(function () {
        const container = document.querySelector('.swagger-ui .topbar');
        if (container && !document.getElementById('custom-help')) {
            const helpText = document.createElement('div');
            helpText.id = 'custom-help';
            helpText.style.cssText = `
                background: #1e3a8a;
                color: white;
                padding: 10px;
                text-align: center;
                font-size: 14px;
                border-bottom: 1px solid #374151;
            `;
            helpText.innerHTML = '💡 برای مشاهده API های سرویس‌های مختلف، از dropdown "Select a definition" استفاده کنید';
            container.parentNode.insertBefore(helpText, container.nextSibling);
        }
    }, 2000);
});