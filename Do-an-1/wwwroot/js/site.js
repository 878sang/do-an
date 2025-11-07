// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


(function() {
    var MINICART_PANEL_SELECTOR = '.offCanvas__minicart';
    var BODY_ACTIVE_CLASS = 'offCanvas__minicart_active';

    function openMinicart() {
        var panel = document.querySelector(MINICART_PANEL_SELECTOR);
        if (panel && !panel.classList.contains('active')) {
            panel.classList.add('active');
        }
        document.body.classList.add(BODY_ACTIVE_CLASS);
    }

    function closeMinicart() {
        var panel = document.querySelector(MINICART_PANEL_SELECTOR);
        if (panel && panel.classList.contains('active')) {
            panel.classList.remove('active');
        }
        document.body.classList.remove(BODY_ACTIVE_CLASS);
    }

    document.addEventListener('click', function(e) {
        var openBtn = e.target.closest && e.target.closest('.minicart__open--btn');
        if (openBtn) {
            e.preventDefault();
            openMinicart();
            return;
        }

        var closeBtn = e.target.closest && e.target.closest('.minicart__close--btn');
        if (closeBtn) {
            e.preventDefault();
            closeMinicart();
        }
    });
})();

$(document).on('click', '.add__to--cart', function(e) {
    e.preventDefault();
    var productId = $(this).data('id');
    $.ajax({
        url: '/Cart/AddToCart',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: productId }),
        success: function(res) {
            if (res.success) {
                $.get('/Cart/MiniCart', function(html) {
                    var minicartWrap = $('.minicart__product').parent();
                    if (minicartWrap.length > 0) {
                        minicartWrap.html(html);
                    }
                });
            } else {
                alert(res.message);
            }
        }
    });
});

// Mini cart: increase / decrease quantity
// Chỉ xử lý buttons trong minicart để tránh conflict với script.js
// Event listener được thêm ngay lập tức để chạy trước script.js
(function() {
    // Thêm event listener ngay lập tức (không đợi DOM ready) để đảm bảo chạy trước script.js
    document.addEventListener('click', function(e) {
        // Tìm button quantity (có thể click vào text node bên trong button)
        var button = e.target.closest && e.target.closest('.quantity__value');
        if (!button) return;
        
        // Chỉ xử lý nếu là button increase hoặc decrease
        if (!button.classList || (!button.classList.contains('increase') && !button.classList.contains('decrease'))) {
            return;
        }
        
        // Chỉ xử lý nếu button nằm trong minicart (có parent với class minicart__quantity)
        var minicartContainer = button.closest && button.closest('.minicart__quantity');
        if (!minicartContainer) return; // Không phải trong minicart, để script.js xử lý

        // Ngăn script.js xử lý - dùng stopImmediatePropagation để chặn các listener khác
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();

        var productId = parseInt(button.getAttribute('data-id'), 10);
        if (!productId) {
            console.error('ProductId not found');
            return;
        }

        var wrapper = button.closest && button.closest('.quantity__box');
        var input = wrapper && wrapper.querySelector('.quantity__number');
        if (!input) {
            console.error('Input not found for productId:', productId);
            return;
        }

        var current = parseInt(input.value, 10) || 1;
        if (button.classList.contains('increase')) {
            current++;
        } else {
            current = Math.max(1, current - 1);
        }
        
        // Gọi API để update quantity
        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ id: productId, quantity: current })
        })
        .then(function(res) {
            return res.json();
        })
        .then(function(res) {
            if (res.success) {
                input.value = current; // Update input field
                var totalElement = document.getElementById('minicart-total');
                if (totalElement) {
                    totalElement.textContent = res.total.toLocaleString('vi-VN');
                }
                if (typeof window.setCartCount === 'function' && typeof res.cartCount !== 'undefined') {
                    window.setCartCount(res.cartCount);
                }
            } else {
                console.error('Update failed:', res.message);
            }
        })
        .catch(function(error) {
            console.error('Error updating quantity:', error);
        });
    }, true); // Use capture phase để chạy trước script.js
})();

// Mini cart: manual quantity change
$(document).on('change', '.quantity__number', function(e) {
    var productId = $(this).data('id');
    var qty = parseInt($(this).val()) || 1;
    if (qty < 1) qty = 1;
    $.ajax({
        url: '/Cart/UpdateQuantity',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: productId, quantity: qty }),
        success: function(res) {
            if (res.success) {
                $('#minicart-total').text(res.total.toLocaleString('vi-VN')); // Update total price
                if (typeof window.setCartCount === 'function' && typeof res.cartCount !== 'undefined') {
                    window.setCartCount(res.cartCount);
                }
            }
        }
    });
});

// Mini cart: remove item
$(document).on('click', '.minicart__product--remove', function(e) {
    e.preventDefault();
    var productId = $(this).data('id');
    $.ajax({
        url: '/Cart/RemoveFromCart',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: productId }),
        success: function(res) {
            if (res.success) {
                $.get('/Cart/MiniCart', function(html) {
                    var minicartWrap = $('.minicart__product').parent();
                    if (minicartWrap.length > 0) {
                        minicartWrap.html(html);
                    }
                });
                if (typeof window.setCartCount === 'function' && typeof res.cartCount !== 'undefined') {
                    window.setCartCount(res.cartCount);
                }
            }
        }
    });
});

function updateMiniCartTotal() {
    var total = 0;
    $('.minicart__product--items').each(function() {
        var price = parseFloat($(this).find('.current__price').data('price')) || 0;
        var qty = parseInt($(this).find('.quantity__number').val()) || 0;
        total += price * qty;
    });
    $('#minicart-total').text(total.toLocaleString('vi-VN'));
    return total;
}