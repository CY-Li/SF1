Vue.component('my-header', {
  template: `
  <div class="navBar">
  <div class="logo"><a href="/"><img src="./images/logo.png" alt=""></a></div>
  <div class="navs">
    <div class="closeMenu">
      <span><i class="bi bi-dash"></i></span>
    </div>
    <a id="notLogin_mobile" href="/login.html" target="_self" class="nav registNav">登入 ∕ 註冊</a>
    <a href="/" target="_self" class="nav homeNav">首　　頁</a>
    <a href="/hall.html" target="_self" class="nav hallNav">商會大廳</a>
    <a href="/wallet.html" target="_self" class="nav walletNav">會員錢包</a>
    <a href="/account.html" target="_self" class="nav mainNav">會員資訊</a>
    <div class="menuFooter text-en">Ping An Chamber of commerce</div>
  </div>
  <a id="notLogin" class="registBtn" target="_self" href="/login.html">登入 / 註冊</a>
  <div id="isLogin" class="dropdown d-none">
    <div class="registBtn" data-bs-toggle="dropdown"></div>
    <ul class="dropdown-menu text-center" style="background: #a11f1a">
      <a href="/account.html" class="text-white" target="_self">會員資訊</a>
      <div class="text-white my-2" id="logoutBtn" style="cursor: pointer;">登出</div>
    </ul>
  </div>
  <span class="hamburger"><i class="bi bi-list"></i></span>
</div>
  `
});

const headerControl = {
  data() {
    return {
      userData: {},
      isLogin: false,
    }
  },
  methods: {


  },
  mounted() {
    const userData = JSON.parse(localStorage.getItem("mm_user_data"))
    if (userData && userData.accessToken) {
      this.isLogin = true
      this.userData = userData
      $("#notLogin").addClass("d-none")
      $("#notLogin_mobile").addClass("d-none")
      $("#isLogin").removeClass("d-none")
      $("#isLogin .registBtn").text(userData.mm_name)
    }

    let vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty("--vh", `${vh}px`);
    window.onresize = (event) => {
      let vh = window.innerHeight * 0.01;
      document.documentElement.style.setProperty("--vh", `${vh}px`);
    };
    $(".navBar .hamburger").click(function() {
      $(".navs").slideDown()
      $("body").addClass("lock-body")
    })
    $(".navBar .closeMenu").click(function() {
      $(".navs").slideUp()
      $("body").removeClass("lock-body")
    })
    $("#logoutBtn").click(function() {
      localStorage.removeItem("mm_user_data")
      window.location.href = "./index.html"
    }) 
  }
}