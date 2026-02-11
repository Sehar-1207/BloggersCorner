// This function will be globally available
function showToast(message, type = 'info', delay = 5000) {
    // If message is empty or null, don't show the toast
    if (!message || message.trim() === '') {
        return;
    }

    const toastContainer = $('#toast-container');
    if (toastContainer.length === 0) {
        console.error('Toast container not found. Make sure you have a <div id="toast-container"> in your layout.');
        return;
    }

    const toastId = `custom-toast-${Date.now()}`; // Unique ID for each toast

    let iconClass = '';
    switch (type) {
        case 'success':
            iconClass = 'fa-check-circle';
            break;
        case 'error':
            iconClass = 'fa-exclamation-triangle';
            break;
        case 'warning':
            iconClass = 'fa-solid fa-triangle-exclamation';
            break;
        case 'info':
        default:
            iconClass = 'fa-info-circle';
            break;
    }

    // Create the custom toast HTML structure
    const toastHtml = `
        <div id="${toastId}" class="custom-toast toast-${type}">
            <i class="fas ${iconClass}"></i>
            <span>${message}</span>
        </div>
    `;

    // Append the toast to the container
    toastContainer.append(toastHtml);

    const newToast = $(`#${toastId}`);

    // Fade in the toast
    newToast.css({ 'opacity': 0, 'display': 'flex' }).animate({ opacity: 1 }, 300);

    // Fade out and remove after the specified delay
    setTimeout(function () {
        newToast.animate({ opacity: 0 }, 500, function () {
            $(this).remove();
        });
    }, delay);
}