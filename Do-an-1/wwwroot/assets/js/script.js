"use strict"; const preLoader = function () { let preloaderWrapper = document.getElementById("preloader"); window.onload = () => { preloaderWrapper.classList.add("loaded") } }; preLoader(); var getSiblings = function (elem) { const siblings = []; let sibling = elem.parentNode.firstChild; for (; sibling;)1 === sibling.nodeType && sibling !== elem && siblings.push(sibling), sibling = sibling.nextSibling; return siblings }, slideUp = (target, time) => { const duration = time || 500; target.style.transitionProperty = "height, margin, padding", target.style.transitionDuration = duration + "ms", target.style.boxSizing = "border-box", target.style.height = target.offsetHeight + "px", target.offsetHeight, target.style.overflow = "hidden", target.style.height = 0, window.setTimeout(() => { target.style.display = "none", target.style.removeProperty("height"), target.style.removeProperty("overflow"), target.style.removeProperty("transition-duration"), target.style.removeProperty("transition-property") }, duration) }, slideDown = (target, time) => { const duration = time || 500; target.style.removeProperty("display"); let display = window.getComputedStyle(target).display; "none" === display && (display = "block"), target.style.display = display; const height = target.offsetHeight; target.style.overflow = "hidden", target.style.height = 0, target.offsetHeight, target.style.boxSizing = "border-box", target.style.transitionProperty = "height, margin, padding", target.style.transitionDuration = duration + "ms", target.style.height = height + "px", window.setTimeout(() => { target.style.removeProperty("height"), target.style.removeProperty("overflow"), target.style.removeProperty("transition-duration"), target.style.removeProperty("transition-property") }, duration) }; function TopOffset(el) { let rect = el.getBoundingClientRect(), scrollTop = window.pageYOffset || document.documentElement.scrollTop; return { top: rect.top + scrollTop } } const headerStickyWrapper = document.querySelector("header"), headerStickyTarget = document.querySelector(".header__sticky"); if (headerStickyTarget) { let headerHeight = headerStickyWrapper.clientHeight; window.addEventListener("scroll", (function () { let StickyTargetElement, TargetElementTopOffset = TopOffset(headerStickyWrapper).top; window.scrollY > TargetElementTopOffset ? headerStickyTarget.classList.add("sticky") : headerStickyTarget.classList.remove("sticky") })) } const scrollTop = document.getElementById("scroll__top"); scrollTop && (scrollTop.addEventListener("click", (function () { window.scroll({ top: 0, left: 0, behavior: "smooth" }) })), window.addEventListener("scroll", (function () { window.scrollY > 300 ? scrollTop.classList.add("active") : scrollTop.classList.remove("active") }))); var swiper = new Swiper(".hero__slider--activation", { slidesPerView: 1, loop: !0, clickable: !0, speed: 800, spaceBetween: 30, autoplay: { delay: 3e3, disableOnInteraction: !1 }, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" } }), swiper = new Swiper(".product__swiper--activation", { slidesPerView: 5, loop: !0, clickable: !0, spaceBetween: 30, breakpoints: { 1200: { slidesPerView: 5 }, 992: { slidesPerView: 4 }, 768: { slidesPerView: 3, spaceBetween: 30 }, 280: { slidesPerView: 2, spaceBetween: 20 }, 0: { slidesPerView: 1 } }, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" } }), swiper = new Swiper(".product__swiper--column4__activation", { slidesPerView: 4, loop: !0, clickable: !0, spaceBetween: 30, breakpoints: { 1200: { slidesPerView: 4 }, 992: { slidesPerView: 4 }, 768: { slidesPerView: 3, spaceBetween: 30 }, 280: { slidesPerView: 2, spaceBetween: 20 }, 0: { slidesPerView: 1 } }, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" } }), swiper = new Swiper(".product__sidebar--column4__activation", { slidesPerView: 4, loop: !0, clickable: !0, spaceBetween: 30, breakpoints: { 1200: { slidesPerView: 4 }, 992: { slidesPerView: 3 }, 768: { slidesPerView: 3, spaceBetween: 30 }, 280: { slidesPerView: 2, spaceBetween: 20 }, 0: { slidesPerView: 1 } }, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" } }), swiper = new Swiper(".product__swiper--column3", { slidesPerView: 3, clickable: !0, loop: !0, spaceBetween: 30, breakpoints: { 1200: { slidesPerView: 3 }, 992: { slidesPerView: 2 }, 768: { slidesPerView: 2, spaceBetween: 30 }, 280: { slidesPerView: 2, spaceBetween: 20 }, 0: { slidesPerView: 1 } }, navigation: { nextEl: ".new__product--sidebar .swiper-button-next", prevEl: ".new__product--sidebar .swiper-button-prev" } }), swiper = new Swiper(".testimonial__swiper--activation", { slidesPerView: 3, loop: !0, clickable: !0, spaceBetween: 30, breakpoints: { 1200: { slidesPerView: 3 }, 768: { spaceBetween: 30, slidesPerView: 2 }, 576: { slidesPerView: 2, spaceBetween: 20 }, 0: { slidesPerView: 1 } }, pagination: { el: ".swiper-pagination", clickable: !0 } }), swiper = new Swiper(".testimonial__activation--column1", { slidesPerView: 1, loop: !0, clickable: !0, pagination: { el: ".swiper-pagination", clickable: !0 } }), swiper = new Swiper(".blog__swiper--activation", { slidesPerView: 4, loop: !0, clickable: !0, spaceBetween: 30, breakpoints: { 1200: { slidesPerView: 4 }, 992: { slidesPerView: 3 }, 768: { slidesPerView: 3, spaceBetween: 30 }, 480: { slidesPerView: 2, spaceBetween: 20 }, 0: { slidesPerView: 1 } }, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" } }), swiper = new Swiper(".quickview__swiper--activation", { slidesPerView: 1, loop: !0, clickable: !0, spaceBetween: 30, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" }, pagination: { el: ".swiper-pagination", clickable: !0 } }), swiper = new Swiper(".product__media--nav", { loop: !0, spaceBetween: 10, slidesPerView: 5, freeMode: !0, watchSlidesProgress: !0, breakpoints: { 768: { slidesPerView: 5 }, 480: { slidesPerView: 4 }, 320: { slidesPerView: 3 }, 200: { slidesPerView: 2 }, 0: { slidesPerView: 1 } }, navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" } }), swiper2 = new Swiper(".product__media--preview", { loop: !0, spaceBetween: 10, thumbs: { swiper: swiper } }); const tab = function (wrapper) { let tabContainer = document.querySelector(wrapper); tabContainer && tabContainer.addEventListener("click", (function (evt) { let listItem = evt.target; if (listItem.hasAttribute("data-toggle")) { let targetId = listItem.dataset.target, targetItem = document.querySelector(targetId); listItem.parentElement.querySelectorAll('[data-toggle="tab"]').forEach((function (list) { list.classList.remove("active") })), listItem.classList.add("active"), targetItem.classList.add("active"), setTimeout((function () { targetItem.classList.add("show") }), 150), getSiblings(targetItem).forEach((function (pane) { pane.classList.remove("show"), setTimeout((function () { pane.classList.remove("active") }), 150) })) } })) }; tab(".product__tab--one"), tab(".product__tab--two"), tab(".product__details--tab"), tab(".product__grid--column__buttons"), document.querySelectorAll("[data-countdown]").forEach((function (elem) { const countDownItem = function (value, label) { return `<div class="countdown__item" ${label}"><span class="countdown__number">${value}</span><p class="countdown__text">${label}</p></div>` }, date = new Date(elem.getAttribute("data-countdown")).getTime(), second = 1e3, minute = 6e4, hour = 36e5, day = 864e5, countDownInterval = setInterval((function () { let currentTime = (new Date).getTime(), timeDistance = date - currentTime, daysValue = Math.floor(timeDistance / day), hoursValue = Math.floor(timeDistance % day / 36e5), minutesValue = Math.floor(timeDistance % 36e5 / 6e4), secondsValue = Math.floor(timeDistance % 6e4 / 1e3); elem.innerHTML = countDownItem(daysValue, "days") + countDownItem(hoursValue, "hrs") + countDownItem(minutesValue, "mins") + countDownItem(secondsValue, "secs"), timeDistance < 0 && clearInterval(countDownInterval) }), 1e3) })); const activeClassAction = function (toggle, target) { const to = document.querySelector(toggle), ta = document.querySelector(target); to && ta && (to.addEventListener("click", (function (e) { e.preventDefault(); let triggerItem = e.target; triggerItem.classList.contains("active") ? (triggerItem.classList.remove("active"), ta.classList.remove("active")) : (triggerItem.classList.add("active"), ta.classList.add("active")) })), document.addEventListener("click", (function (event) { event.target.closest(toggle) || event.target.classList.contains(toggle.replace(/\./, "")) || event.target.closest(target) || event.target.classList.contains(target.replace(/\./, "")) || (to.classList.remove("active"), ta.classList.remove("active")) }))) }; function offcanvsSidebar(openTrigger, closeTrigger, wrapper) { let OpenTriggerprimary__btn = document.querySelectorAll(openTrigger), closeTriggerprimary__btn = document.querySelector(closeTrigger), WrapperSidebar = document.querySelector(wrapper), wrapperOverlay = wrapper.replace(".", ""); function handleBodyClass(evt) { let eventTarget = evt.target; eventTarget.closest(wrapper) || eventTarget.closest(openTrigger) || (WrapperSidebar.classList.remove("active"), document.querySelector("body").classList.remove(`${wrapperOverlay}_active`)) } OpenTriggerprimary__btn && WrapperSidebar && OpenTriggerprimary__btn.forEach((function (singleItem) { singleItem.addEventListener("click", (function (e) { null != e.target.dataset.offcanvas && (WrapperSidebar.classList.add("active"), document.querySelector("body").classList.add(`${wrapperOverlay}_active`), document.body.addEventListener("click", handleBodyClass.bind(this))) })) })), closeTriggerprimary__btn && WrapperSidebar && closeTriggerprimary__btn.addEventListener("click", (function (e) { null != e.target.dataset.offcanvas && (WrapperSidebar.classList.remove("active"), document.querySelector("body").classList.remove(`${wrapperOverlay}_active`), document.body.removeEventListener("click", handleBodyClass.bind(this))) })) } activeClassAction(".account__currency--link", ".dropdown__currency"), activeClassAction(".language__switcher", ".dropdown__language"), activeClassAction(".offcanvas__language--switcher", ".offcanvas__dropdown--language"), activeClassAction(".offcanvas__account--currency__menu", ".offcanvas__account--currency__submenu"), activeClassAction(".footer__language--link", ".footer__dropdown--language"), activeClassAction(".footer__currency--link", ".footer__dropdown--currency"), offcanvsSidebar(".minicart__open--btn", ".minicart__close--btn", ".offCanvas__minicart"), offcanvsSidebar(".search__open--btn", ".predictive__search--close__btn", ".predictive__search--box"), offcanvsSidebar(".widget__filter--btn", ".offcanvas__filter--close", ".offcanvas__filter--sidebar"); const offcanvasHeader = function () { const offcanvasOpen = document.querySelector(".offcanvas__header--menu__open--btn"), offcanvasClose = document.querySelector(".offcanvas__close--btn"), offcanvasHeader = document.querySelector(".offcanvas__header"), offcanvasMenu = document.querySelector(".offcanvas__menu"), body = document.querySelector("body"); offcanvasMenu && offcanvasMenu.querySelectorAll(".offcanvas__sub_menu").forEach((function (ul) { const subMenuToggle = document.createElement("button"); subMenuToggle.classList.add("offcanvas__sub_menu_toggle"), ul.parentNode.appendChild(subMenuToggle) })), offcanvasOpen && offcanvasOpen.addEventListener("click", (function (e) { e.preventDefault(), null != e.target.dataset.offcanvas && (offcanvasHeader.classList.add("open"), body.classList.add("mobile_menu_open")) })), offcanvasClose && offcanvasClose.addEventListener("click", (function (e) { e.preventDefault(), null != e.target.dataset.offcanvas && (offcanvasHeader.classList.remove("open"), body.classList.remove("mobile_menu_open")) })); let mobileMenuWrapper = document.querySelector(".offcanvas__menu_ul"); mobileMenuWrapper && mobileMenuWrapper.addEventListener("click", (function (e) { let targetElement = e.target; if (targetElement.classList.contains("offcanvas__sub_menu_toggle")) { const parent = targetElement.parentElement; parent.classList.contains("active") ? (targetElement.classList.remove("active"), parent.classList.remove("active"), parent.querySelectorAll(".offcanvas__sub_menu").forEach((function (subMenu) { subMenu.parentElement.classList.remove("active"), subMenu.nextElementSibling.classList.remove("active"), slideUp(subMenu) }))) : (targetElement.classList.add("active"), parent.classList.add("active"), slideDown(targetElement.previousElementSibling), getSiblings(parent).forEach((function (item) { item.classList.remove("active"), item.querySelectorAll(".offcanvas__sub_menu").forEach((function (subMenu) { subMenu.parentElement.classList.remove("active"), subMenu.nextElementSibling.classList.remove("active"), slideUp(subMenu) })) }))) } })), offcanvasHeader && document.addEventListener("click", (function (event) { event.target.closest(".offcanvas__header--menu__open--btn") || event.target.classList.contains(".offcanvas__header--menu__open--btn".replace(/\./, "")) || event.target.closest(".offcanvas__header") || event.target.classList.contains(".offcanvas__header".replace(/\./, "")) || (offcanvasHeader.classList.remove("open"), body.classList.remove("mobile_menu_open")) })), offcanvasHeader && window.addEventListener("resize", (function () { window.outerWidth >= 992 && (offcanvasHeader.classList.remove("open"), body.classList.remove("mobile_menu_open")) })) }; offcanvasHeader(); const quantityWrapper = document.querySelectorAll(".quantity__box"); quantityWrapper && quantityWrapper.forEach((function (singleItem) { let increaseButton = singleItem.querySelector(".increase"), decreaseButton = singleItem.querySelector(".decrease"); increaseButton.addEventListener("click", (function (e) { let input = e.target.previousElementSibling.children[0]; if (null != input.dataset.counter) { let value = parseInt(input.value, 10); value = isNaN(value) ? 0 : value, value++, input.value = value } })), decreaseButton.addEventListener("click", (function (e) { let input = e.target.nextElementSibling.children[0]; if (null != input.dataset.counter) { let value = parseInt(input.value, 10); value = isNaN(value) ? 0 : value, value < 1 && (value = 1), value--, input.value = value } })) })); const openEls = document.querySelectorAll("[data-open]"), closeEls = document.querySelectorAll("[data-close]"), isVisible = "is-visible"; for (const el of openEls) el.addEventListener("click", (function () { const modalId = this.dataset.open; document.getElementById(modalId).classList.add(isVisible) })); for (const el of closeEls) el.addEventListener("click", (function () { this.parentElement.parentElement.parentElement.classList.remove(isVisible) })); function customAccordion(accordionWrapper, singleItem, accordionBody) { let accoridonButtons; document.querySelectorAll(accordionWrapper).forEach((function (item) { item.addEventListener("click", (function (evt) { let itemTarget = evt.target; if (itemTarget.classList.contains("accordion__items--button") || itemTarget.classList.contains("widget__categories--menu__label")) { let singleAccordionWrapper = itemTarget.closest(singleItem), singleAccordionBody = singleAccordionWrapper.querySelector(accordionBody); singleAccordionWrapper.classList.contains("active") ? (singleAccordionWrapper.classList.remove("active"), slideUp(singleAccordionBody)) : (singleAccordionWrapper.classList.add("active"), slideDown(singleAccordionBody), getSiblings(singleAccordionWrapper).forEach((function (item) { let sibllingSingleAccordionBody = item.querySelector(accordionBody); item.classList.remove("active"), slideUp(sibllingSingleAccordionBody) }))) } })) })) } document.addEventListener("click", e => { e.target == document.querySelector(".modal.is-visible") && document.querySelector(".modal.is-visible").classList.remove(isVisible) }), document.addEventListener("keyup", e => { "Escape" == e.key && document.querySelector(".modal.is-visible") && document.querySelector(".modal.is-visible").classList.remove(isVisible) }), customAccordion(".accordion__container", ".accordion__items", ".accordion__items--body"), customAccordion(".widget__categories--menu", ".widget__categories--menu__list", ".widget__categories--sub__menu"); let accordion = !0; const footerWidgetAccordion = function () { let footerWidgetContainer; accordion = !1, document.querySelector(".main__footer").addEventListener("click", (function (evt) { let singleItemTarget = evt.target; if (singleItemTarget.classList.contains("footer__widget--button")) { const footerWidget = singleItemTarget.closest(".footer__widget"), footerWidgetInner = footerWidget.querySelector(".footer__widget--inner"); footerWidget.classList.contains("active") ? (footerWidget.classList.remove("active"), slideUp(footerWidgetInner)) : (footerWidget.classList.add("active"), slideDown(footerWidgetInner), getSiblings(footerWidget).forEach((function (item) { const footerWidgetInner = item.querySelector(".footer__widget--inner"); item.classList.remove("active"), slideUp(footerWidgetInner) }))) } })) }; window.addEventListener("load", (function () { accordion && footerWidgetAccordion() })), window.addEventListener("resize", (function () { document.querySelectorAll(".footer__widget").forEach((function (item) { window.outerWidth >= 768 && (item.classList.remove("active"), item.querySelector(".footer__widget--inner").style.display = "") })), accordion && footerWidgetAccordion() })); const customLightboxHTML = '<div id="glightbox-body" class="glightbox-container">\n    <div class="gloader visible"></div>\n    <div class="goverlay"></div>\n    <div class="gcontainer">\n    <div id="glightbox-slider" class="gslider"></div>\n    <button class="gnext gbtn" tabindex="0" aria-label="Next" data-customattribute="example">{nextSVG}</button>\n    <button class="gprev gbtn" tabindex="1" aria-label="Previous">{prevSVG}</button>\n    <button class="gclose gbtn" tabindex="2" aria-label="Close">{closeSVG}</button>\n    </div>\n    </div>', lightbox = GLightbox({ touchNavigation: !0, lightboxHTML: customLightboxHTML, loop: !0 }), wrapper = document.getElementById("funfactId"); if (wrapper) { const counters = wrapper.querySelectorAll(".js-counter"), duration = 1e3; let isCounted = !1; document.addEventListener("scroll", (function () { const wrapperPos = wrapper.offsetTop - window.innerHeight; !isCounted && window.scrollY > wrapperPos && (counters.forEach(counter => { const countTo = counter.dataset.count, countPerMs = countTo / duration; let currentCount = 0; const countInterval = setInterval((function () { currentCount >= countTo && clearInterval(countInterval), counter.textContent = Math.round(currentCount), currentCount += countPerMs }), 1) }), isCounted = !0) })) } const newsletterPopup = function () { let newsletterWrapper = document.querySelector(".newsletter__popup"), newsletterCloseButton = document.querySelector(".newsletter__popup--close__btn"), dontShowPopup = document.querySelector("#newsletter__dont--show"), popuDontShowMode = localStorage.getItem("newsletter__show"); newsletterWrapper && null == popuDontShowMode && window.addEventListener("load", event => { setTimeout((function () { document.body.classList.add("overlay__active"), newsletterWrapper.classList.add("newsletter__show"), document.addEventListener("click", (function (event) { event.target.closest(".newsletter__popup--inner") || (document.body.classList.remove("overlay__active"), newsletterWrapper.classList.remove("newsletter__show")) })), newsletterCloseButton.addEventListener("click", (function () { document.body.classList.remove("overlay__active"), newsletterWrapper.classList.remove("newsletter__show") })), dontShowPopup.addEventListener("click", (function () { dontShowPopup.checked ? localStorage.setItem("newsletter__show", !0) : localStorage.removeItem("newsletter__show") })) }), 3e3) }) }; newsletterPopup();
const chatBody = document.querySelector(".chat-body");
const messageInput = document.querySelector(".message-input");
const sendMessageButton = document.querySelector("#send-message");
const fileInput = document.querySelector("#file-input");
const fileUploadWrapper = document.querySelector(".file-upload-wrapper");
const fileCancelButton = document.querySelector("#file-cancel");
const chatbotToggler = document.querySelector("#chatbot-toggler");
const closeChatbot = document.querySelector("#close-chatbot");


// Api setup
const API_KEY = "ĐIỀN API KEY CỦA BẠN VÀO ĐÂY"; // LINK LẤY API KEY: https://aistudio.google.com/apikey
const API_URL = `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=${API_KEY}`;

const userData = {
    message: null,
    file: {
        data: null,
        mime_type: null
    }
};

// const chatHistory = [
//     {
//         role: "model",
//         parts: [{ text: `Đinh Duy Vinh (2005), chàng sinh viên đến từ Quảng Ngãi, hiện đang theo học tại Đại học Duy Tân, Đà Nẵng, là một người trẻ đam mê công nghệ và lập trình. Từ thuở nhỏ, Vinh đã có niềm đam mê mãnh liệt với các thiết bị điện tử và luôn muốn tìm hiểu mọi thứ xung quanh. Chính sự tò mò này đã đưa anh đến với thế giới lập trình ngay từ những năm cấp 3, đặc biệt là trong thời gian giãn cách xã hội do dịch COVID-19. Với thời gian rảnh rỗi, Vinh bắt đầu tự học lập trình web, và rồi từ những dự án nhỏ ban đầu, anh đã phát triển được những sản phẩm hữu ích cho cộng đồng.
// Những dự án mà Vinh thực hiện không chỉ đơn giản là những sản phẩm công nghệ mà còn là minh chứng cho sự sáng tạo và khả năng giải quyết vấn đề của anh. Anh đã tự tay xây dựng một loạt các dự án đa dạng như hệ thống quản lý sinh viên, web game giải trí, website chống lừa đảo, trang web tải ảnh từ Imgur, công cụ tạo mã QR code, dự báo thời tiết trực tuyến, và cả extension Chrome giúp đánh giá nhanh giảng viên của trường Đại học Duy Tân. Không dừng lại ở đó, Vinh còn đắm chìm vào việc khai thác API từ các mạng xã hội như Instagram, Facebook, TikTok và Zalo để lấy thông tin người dùng. Anh cũng đã thử sức với việc tạo module iOS để crack ứng dụng Locket, phát triển API tải video từ TikTok, tạo web chuyển đổi 2FA, và không thể không nhắc đến các bot Telegram mà Vinh viết để tự động hóa các tác vụ một cách hiệu quả.
// Vinh không chỉ giỏi trong việc phát triển các dự án công nghệ mà còn luôn mong muốn chia sẻ những gì mình học được với cộng đồng. Kênh YouTube của anh (YouTube: @duyvinh09) là nơi anh chia sẻ những mẹo, thủ thuật và tiện ích cực kỳ hữu ích mà anh đã tự tìm ra, giúp đỡ mọi người trong hành trình học hỏi công nghệ. Ngoài YouTube, Vinh cũng kết nối và chia sẻ kiến thức qua các nền tảng khác như GitHub (GitHub: duyvinh09) và Facebook (Facebook: duyvinh09), nơi anh luôn sẵn sàng giao lưu, học hỏi từ cộng đồng và giúp đỡ những người có chung niềm đam mê. Đặc biệt, Vinh còn sở hữu một nhóm chat trên Telegram, nơi anh và các bạn có thể trao đổi kiến thức, cùng nhau phát triển và học hỏi từ những người đi trước.
// Với một portfolio đầy ấn tượng tại duyvinh09.github.io và dinhduyvinh.eu.org, Vinh không ngừng khẳng định khả năng của mình qua mỗi dự án. Anh là một chàng trai luôn nỗ lực học hỏi, phát triển và sẵn sàng chia sẻ với cộng đồng những gì anh biết. Với tinh thần sáng tạo không ngừng nghỉ và sự nhiệt huyết trong từng dự án, Đinh Duy Vinh chắc chắn sẽ còn đạt được nhiều thành công và tiếp tục là nguồn cảm hứng cho thế hệ trẻ đam mê công nghệ.` }],
//     },
// ];

const chatHistory = [];

const initialInputHeight = messageInput.scrollHeight;

// Create message element with dynamic classes and return it
const createMessageElement = (content, ...classes) => {
    const div = document.createElement("div");
    div.classList.add("message", ...classes);
    div.innerHTML = content;
    return div;
};

// Generate bot response using API
const generateBotResponse = async (incomingMessageDiv) => {
    const messageElement = incomingMessageDiv.querySelector(".message-text");

    // chatHistory.push({
    //     role: "user",
    //     parts: [{ text: `Using the details provided above, please address this query: ${userData.message}` }, ...(userData.file.data ? [{ inline_data: userData.file }] : [])],
    // });

    chatHistory.push({
        role: "user",
        parts: [{ text: userData.message }, ...(userData.file.data ? [{ inline_data: userData.file }] : [])],
    });

    // API request options
    const requestOptions = {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            contents: chatHistory
        })
    }

    try {
        // Fetch bot response from API
        const response = await fetch(API_URL, requestOptions);
        const data = await response.json();
        if (!response.ok) throw new Error(data.error.message);

        // Extract and display bot's response text
        const apiResponseText = data.candidates[0].content.parts[0].text.replace(/\*\*(.*?)\*\*/g, "$1").trim();
        messageElement.innerText = apiResponseText;
        chatHistory.push({
            role: "model",
            parts: [{ text: apiResponseText }]
        });
    } catch (error) {
        messageElement.innerText = error.message;
        messageElement.style.color = "#ff0000";
    } finally {
        userData.file = {};
        incomingMessageDiv.classList.remove("thinking");
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
    }
};

// Handle outgoing user message
const handleOutgoingMessage = (e) => {
    e.preventDefault();
    userData.message = messageInput.value.trim();
    messageInput.value = "";
    fileUploadWrapper.classList.remove("file-uploaded");
    messageInput.dispatchEvent(new Event("input"));

    // Create and display user message
    const messageContent = `<div class="message-text"></div>
                            ${userData.file.data ? `<img src="data:${userData.file.mime_type};base64,${userData.file.data}" class="attachment" />` : ""}`;

    const outgoingMessageDiv = createMessageElement(messageContent, "user-message");
    outgoingMessageDiv.querySelector(".message-text").innerText = userData.message;
    chatBody.appendChild(outgoingMessageDiv);
    chatBody.scrollTop = chatBody.scrollHeight;

    // Simulate bot response with thinking indicator after a delay
    setTimeout(() => {
        const messageContent = `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">
                    <div class="thinking-indicator">
                        <div class="dot"></div>
                        <div class="dot"></div>
                        <div class="dot"></div>
                    </div>
                </div>`;

        const incomingMessageDiv = createMessageElement(messageContent, "bot-message", "thinking");
        chatBody.appendChild(incomingMessageDiv);
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
        generateBotResponse(incomingMessageDiv);
    }, 600);
};

// Handle Enter key press for sending messages
messageInput.addEventListener("keydown", (e) => {
    const userMessage = e.target.value.trim();
    if (e.key === "Enter" && userMessage && !e.shiftKey && window.innerWidth > 768) {
        handleOutgoingMessage(e);
    }
});

messageInput.addEventListener("input", (e) => {
    messageInput.style.height = `${initialInputHeight}px`;
    messageInput.style.height = `${messageInput.scrollHeight}px`;
    document.querySelector(".chat-form").style.boderRadius = messageInput.scrollHeight > initialInputHeight ? "15px" : "32px";
});

// Handle file input change event
fileInput.addEventListener("change", (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e) => {
        fileUploadWrapper.querySelector("img").src = e.target.result;
        fileUploadWrapper.classList.add("file-uploaded");
        const base64String = e.target.result.split(",")[1];

        // Store file data in userData
        userData.file = {
            data: base64String,
            mime_type: file.type
        };

        fileInput.value = "";
    };

    reader.readAsDataURL(file);
});

fileCancelButton.addEventListener("click", (e) => {
    userData.file = {};
    fileUploadWrapper.classList.remove("file-uploaded");
});

const picker = new EmojiMart.Picker({
    theme: "light",
    showSkinTones: "none",
    previewPosition: "none",
    onEmojiSelect: (emoji) => {
        const { selectionStart: start, selectionEnd: end } = messageInput;
        messageInput.setRangeText(emoji.native, start, end, "end");
        messageInput.focus();
    },
    onClickOutside: (e) => {
        if (e.target.id === "emoji-picker") {
            document.body.classList.toggle("show-emoji-picker");
        } else {
            document.body.classList.remove("show-emoji-picker");
        }
    },
});

document.querySelector(".chat-form").appendChild(picker);

fileInput.addEventListener("change", async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const validImageTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!validImageTypes.includes(file.type)) {
        await Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WEBP)',
            confirmButtonText: 'OK'
        });
        resetFileInput();
        return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
        fileUploadWrapper.querySelector("img").src = e.target.result;
        fileUploadWrapper.classList.add("file-uploaded");
        const base64String = e.target.result.split(",")[1];
        userData.file = {
            data: base64String,
            mime_type: file.type
        };
    };
    reader.readAsDataURL(file);
});

function resetFileInput() {
    fileInput.value = "";
    fileUploadWrapper.classList.remove("file-uploaded");
    fileUploadWrapper.querySelector("img").src = "#";
    userData.file = { data: null, mime_type: null };
    document.querySelector(".chat-form").reset();
}

sendMessageButton.addEventListener("click", (e) => handleOutgoingMessage(e));
document.querySelector("#file-upload").addEventListener("click", (e) => fileInput.click());
chatbotToggler.addEventListener("click", () => document.body.classList.toggle("show-chatbot"));
closeChatbot.addEventListener("click", () => document.body.classList.remove("show-chatbot"));