
const app = new Vue({
  el: "#app",
  mixins: [headerControl],
  data() {
    return {
      layoutType: 1, // 1登入 2註冊 3忘記密碼 4條款
      showPassword: false,
      img: null,
      validCode: "",
      canvasWidth: 160,
      canvasHeight: 40,
      sColor: ["#B22222", "#F9F900", "#82D900", "#FFAF60"],  //干擾線顏色
      fColor: ["#804040", "#5A5AAD", "#408080", "#8F4586"],   //文字顏色,
      loginForm: {
        account: "",
        pswd: "",
        validCode: ""
      },
      registForm: {},
      loading: false
    }
  },
  methods: {
    initRegistForm() {
      this.registForm = {
        account: "",
        pswd: "",
        pswd2: "",
        invite: "",
        mm_2nd_pwd: '',
        mm_2nd_pwd2: '',
        confirm: false,
        gender: "1",
        identify: "",
        address: "",
        email: "",
        areaCode: "+886",
        introducer: ''
      }
    },
    showTerms() {
      console.log(2222)
      this.layoutType = 4;
      this.$nextTick(() => {
        $("html, body").animate( { scrollTop: 0}, 500);
      })
    },
    // 生成隨機顏色組合序號
    randColor() {
      return  Math.floor(Math.random() * this.sColor.length);
    },
    // 干擾線的隨機x坐標值
    lineX() {
      return Math.floor(Math.random() * this.canvasWidth);
    },

    // 干擾線的隨機y坐標值
    lineY() {
      return Math.floor(Math.random() * this.canvasHeight);
    },
    drawCanvas() {
      const mycanvas = document.getElementById('mycanvas');
      const cxt = mycanvas.getContext('2d');
      const codes = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"]
      //重設canvas內容
      mycanvas.width  = mycanvas.width;
      mycanvas.height = mycanvas.height;
      const { canvasWidth, canvasHeight, sColor,  fColor } = this
      //選取底圖範圍
      cxt.drawImage(this.img, this.lineX(), this.lineY(), canvasWidth, canvasHeight, 0, 0, canvasWidth, canvasHeight);
      
      /*生成干擾線2條*/
      for (let i = 0; i < 2; i++) {
        cxt.beginPath(); 
        cxt.strokeStyle = sColor[this.randColor()];
        cxt.moveTo(0, this.lineY());
        cxt.lineTo(canvasWidth, this.lineY());
        cxt.lineWidth = (Math.floor(Math.random() * (20 - 10 + 1)) + 10) / 10;
        cxt.stroke();
        cxt.closePath();
      }
      cxt.font = 'bold 40px Verdana';
      // cxt.letterSpacing = "8px";
      this.validCode = "";
      for (let i = 0; i < 4; i++) {
        const ind = Math.floor(Math.random() * codes.length)
        this.validCode += codes[ind]
        cxt.fillStyle = fColor[this.randColor()];//隨機文字顏色

        cxt.fillText(this.validCode[i], 5 + (i * 38), 35);
      }
    },

    login() {
      if (this.loading) return
      const { account, pswd, validCode } = this.loginForm
      if (this.validCode !== validCode) {
        alert("驗證碼輸入錯誤")
        return
      }
      const body = {
        mm_account: account.replace(/^\+886/, '0'),
        mm_pwd: pswd,
      }
      this.loading = true
      axios.post(baseUrl+"Login", body).then(({ data }) => {
        const { returnStatus, result, returnMsg } = data
        this.loading = false
        if (returnStatus === 1) {
          localStorage.setItem("mm_user_data", JSON.stringify(result))
          window.location.href = "./index.html"
        } else {
          alert(returnMsg)
        }
      })
    },
    regist() {
      if (this.loading) return
      const { account, pswd, pswd2, invite, introducer, areaCode, mm_2nd_pwd, mm_2nd_pwd2 } = this.registForm
      if (!/^\d+-\d+$/.test(account)) {
        alert("帳號格式錯誤，格式須為電話-數字，例如933222111-1")
        return
      }
      if (pswd !== pswd2) {
        alert("密碼輸入錯誤")
        return
      }
      if (mm_2nd_pwd !== mm_2nd_pwd2) {
        alert("二級密碼輸入錯誤")
        return
      }
      this.loading = true
      const phone = (areaCode + account).replace(/^\+886/, '0')
      const body = {
        mm_account: phone,
        mm_pwd: pswd,
        mm_introduce_user: introducer? introducer : 0,
        mm_invite_user: invite? invite : 0,
        mm_2nd_pwd
      }
      axios.post(baseUrl+"Register", body).then(({ data }) => {
        const { returnStatus, returnMsg } = data
        this.loading = false
        if (returnStatus === 1) {
          alert("註冊成功")
          this.initRegistForm()
          this.layoutType = 1
        } else {
          alert(returnMsg)
        }
      })

    }
  },
  created() {
    this.initRegistForm()
  },
  mounted() {
    const url = new URL(location.href).searchParams
    const referee = url.get('referee')
    if (referee) {
      this.layoutType = 2
      // this.registForm.invite = referee
      this.registForm.introducer = referee
    }
    this.img = new Image(); 
    const vm = this;
    this.img.src ="././images/canvas.jpg";
    this.img.onload = function() {
      vm.drawCanvas()
    }
  }
})

