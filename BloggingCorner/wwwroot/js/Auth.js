document.addEventListener('DOMContentLoaded', function () {
    const signUpButton = document.getElementById('signUp');
    const signInButton = document.getElementById('signIn');
    const container = document.getElementById('auth-container');

    if (signUpButton && container) {
        signUpButton.addEventListener('click', () => {
            container.classList.add('right-panel-active');
        });
    }

    if (signInButton && container) {
        signInButton.addEventListener('click', () => {
            container.classList.remove('right-panel-active');
        });
    }
});