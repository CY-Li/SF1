$(function() {
  const url = new URL(window.location);
  const query = url.searchParams.get("type");
  let currrentPage = url.searchParams.get("page") || 1; // 當前資料頁面
  currrentPage = Number(currrentPage);
  let pageGroup = currrentPage; // 分頁顯示的群組

  const navQuerys = ["recharge", "withdraw", "detail", "closing"]
  if (navQuerys.indexOf(query) >= 0) {
    if (query == "closing") {
      defineClosingPagination(window.innerWidth > 992? 8 : 3);
    }
  }

  // 銀行轉帳方式切換
  let transferMethod = "fillIn";
  $("#transfer_method").on("change", function() {
    transferMethod = $(this).val();
    if (transferMethod == "fillIn") {
      $(".accountForm").removeClass("d-none");
      $(".qrcodeForm").addClass("d-none");
    } else if (transferMethod == "qrcode") {
      $(".qrcodeForm").removeClass("d-none");
      $(".accountForm").addClass("d-none");
    }
  })

  // 切換分頁
  function togglePage(type, page) {
    window.location.href = `/wallet.html?type=${type}&page=${page}`;
  }

  // 死會會費分頁頁籤(需要顯示的數字組數)
  function defineClosingPagination(pageNum) {
    const totalPage = $("#closingForm .page").length;
    const group = Math.ceil(pageGroup / pageNum);
    // 往前/後切頁按鈕
    if (currrentPage == 1) {
      $("#closingForm .page-prev").addClass("disabled");
    } else if (currrentPage == totalPage) {
      $("#closingForm .page-next").addClass("disabled");
    }
    // 切換頁籤群組按鈕
    $("#closingForm .page-ellipsis-prev")[group == 1? "addClass" : "removeClass"]("d-none");
    $("#closingForm .page-ellipsis-next")[group * pageNum >= totalPage? "addClass" : "removeClass"]("d-none");
    
    // 分頁是否顯示
    $("#closingForm .page").each(function() {
      const page = $(this).data("page");
      if (page == currrentPage) $(this).addClass("active")
      if ((page > (pageNum * (group - 1)) && page <= pageNum * group)) {
        $(this).removeClass("d-none")
      } else {
        $(this).addClass("d-none")
      }
    })
  }
  // 定義金流明細切頁事件
  function closingPageEvent() {
    $("#closingForm .page").click(function() {
      const pageNum = $(this).data("page");
      if (pageNum == currrentPage) return
      togglePage("closing", pageNum)
    })
    $("#closingForm .page-prev").not(".disabled").click(function() {
      togglePage("closing", currrentPage - 1)
    })
    $("#closingForm .page-next").not(".disabled").click(function() {
      togglePage("closing", currrentPage + 1)
    })
    $("#closingForm .page-ellipsis-prev").click(function() {
      const pageNum = window.innerWidth > 992? 8 : 3;
      pageGroup -= pageNum;
      defineClosingPagination(pageNum);
    })
    $("#closingForm .page-ellipsis-next").click(function() {
      const pageNum = window.innerWidth > 992? 8 : 3;
      pageGroup += pageNum;
      defineClosingPagination(pageNum);
    })
  }


  $(".tableRow .toggleBtn").click(function() {
    $(this).toggleClass("open")
    $(this).parent(".rightRow").siblings(".detail").slideToggle()
  })

  closingPageEvent();
})

const app = new Vue({
  el: "#app",
  mixins: [headerControl],
  data() {
    return {
      userData: {},
      walletData: {},
      navQuerys: ["recharge", "withdraw", "detail", "closing"],
      urlType: "recharge",
      payData: {
        records: []
      },
      rechargeMethod: "", // 加值方式
      depositData: {}, // 加值時要用的參數
      blockchainForm: { coinType: "NTD", amount: "", key: "", confirm: false },
      bankForm: { amount: "", key: "",},
      rechargeRecords: [],
      cashFlowData: [],
      cashFlowTotalCount: 0,
      cashFlowTotalPage: 0,
      isAuto: true,
      pageIndex: 1,
      pageSize: 10,
      pageGroupSize: 0,
      currentPageSizeGroup: 0,
      withdrawForm: {
        bank: "1",
        account: '',
        amount: ''
      },
      pointType: 0,
      registPoint: undefined,
      registPointTarget: '',
      isLoading: false,
      mm_2nd_pwd: '',
      file1: { base64: '', file: null },
      file2: { base64: '', file: null },
      conf: {}
    }
  },
  computed: { 
    rechargeBtnDisabled() {
      const { rechargeMethod } = this
      if (rechargeMethod == 'blockchain') {
        const { coinType, amount, key, confirm } = this.blockchainForm
        return !key || !coinType || amount <= 0 || !confirm
      } else if (rechargeMethod == 'bank') {
        return false
      } else {
        return true
      }
    },
    hasPreGroup() {
      return this.currentPageSizeGroup > 1
    },
    hasNextGroup() {
      const { pageGroupSize, currentPageSizeGroup, cashFlowTotalPage } = this
      return currentPageSizeGroup < Math.ceil(cashFlowTotalPage / pageGroupSize)
    }

  },
  methods: {
    getUserData() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"Members/"+userData.mm_id).then(({ data }) => {
          const { returnStatus, result, returnMsg } = data
          if (returnStatus === 1) {
            this.userData = result
          } else {
            alert(returnMsg)
          }
        })

      } else {
        location.href = "./login.html"
      }
    },
    getWallet() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"Wallet/"+userData.mm_id).then(({ data }) => {
          const { returnStatus, result, returnMsg } = data
          if (returnStatus === 1) {
            this.walletData = result
          } else {
            alert(returnMsg)
          }
        })

      }
    },
    // 取得儲值紀錄
    getRechargeRecords() {
      // axios.post(baseUrl+"ApplyDeposit/GetApplyDepositLisForUser", { ad_mm_id: this.userData.mm_id }).then(({ data }) => {
      //   this.rechargeRecords = data.result
      // })
    },
    // 取儲值QR CODE及流水號
    getDepositData() {
      axios.get(baseUrl+"ApplyDeposit/GetDepositData").then(({ data }) => {
        this.depositData = data.result
      })
    },
    // 進行儲值
    doDeposit() {
      // const { rechargeMethod } = this
      // if (rechargeMethod == 'blockchain') this.depositByBlockChain()
      this.depositByBlockChain()
    },
    depositByBlockChain() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      const { mm_id } = userData
      const { key, address } = this.blockchainForm
      const { sps_picture, sps_parameter01 } = this.depositData
      const isBlockChain = this.rechargeMethod == 'blockchain'
      const ad_type = isBlockChain? 10 : 50
      const formData = new FormData()
      formData.append("ad_mm_id", mm_id)
      formData.append("ad_amount", isBlockChain? this.blockchainForm.amount : this.bankForm.amount)
      formData.append("ad_key", key)
      formData.append("ad_url", sps_parameter01)
      formData.append("ad_picture", sps_picture)
      formData.append("ad_akc_mw_address", address)
      formData.append("ad_file_image", isBlockChain ? this.file1.file : this.file2.file)
      formData.append("ad_type", ad_type)
      formData.append("ad_rate", this.conf.deposit_rate)
      axios.post(baseUrl+"ApplyDeposit",  formData, { headers: { 'Content-Type': 'multipart/form-data' }}).then(({ data }) => {
        const { returnMsg, result } = data
        if (result) {
          this.getRechargeRecords()
          this.blockchainForm = { coinType: "NTD", amount: "", key: "", confirm: false, address: '' }
          this.file1 = { base64: '', file: null }
          this.file2 = { base64: '', file: null }
        }
        alert(returnMsg)
      })
    },
    changePointType() {
      this.pageIndex = 1
      this.getMemberBalance()
    },
    getMemberBalance() {
      const { pageIndex, pageSize, pageGroupSize, pointType } = this;
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      const params = {
        mb_mm_id: userData.mm_id,
        pageIndex,
        pageSize
      }
      if (this.urlType == 'closing') {
        params.mb_points_type = 16
      } else if (pointType) {
        params.mb_points_type = pointType
      }
      axios.get(baseUrl+"Balances/QueryMemberBalanceUser", { params }).then(({ data }) => {
        if (data.result) {
          this.cashFlowData = data.result.gridRespResult
          this.cashFlowTotalCount = data.result.gridTotalCount
          this.cashFlowTotalPage = Math.ceil(this.cashFlowTotalCount / pageSize)
          this.currentPageSizeGroup = Math.ceil(pageIndex / pageGroupSize)
        }
      })
    },
    isInCurrentPageGroup(page) {
      const  { currentPageSizeGroup, pageGroupSize } = this
      return page > (currentPageSizeGroup - 1) * pageGroupSize && page <= currentPageSizeGroup * pageGroupSize
      
    },
    changePage(page) {
      const { pageIndex, cashFlowTotalPage } = this
      if (page == pageIndex || page < 1 || page > cashFlowTotalPage) return
      this.pageIndex = page
      this.getMemberBalance()
    },
    checkUrl() {
      const url = new URL(window.location);
      const query = url.searchParams.get("type");
      if (query) this.urlType = query
      const { urlType } = this
      if (urlType == 'recharge') this.getRechargeRecords()
      if (urlType == 'detail' || urlType == 'closing') this.getMemberBalance()
    },
    autoPay() {
      this.$nextTick(() => {
        this.isAuto = true
        alert("請聯絡客服")
      })
    },
    doWithdraw() {
      const reg = /^[1-9]\d*$/
      const { account, amount } = this.withdrawForm
      if (!account || !amount) {
        alert("請輸入帳號及提領金額")
        return
      }
      if (!reg.test(amount)) {
        alert("提領金額輸入有誤")
        return
      }
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      const { mm_id } = userData.mm_id
console.log(this.conf.mm_kyc_id);
console.log(mm_id );

      const body = {
        aw_mm_id: mm_id,
        aw_amount: Number(amount),
        aw_wallet_address: account,
        aw_kyc_id: this.conf.mm_kyc_id,
        aw_rate: this.conf.withdraw_rate,
        aw_type: 50
      }
      axios.post(baseUrl+"ApplyWithdraw", body).then(({ data }) => {
        const { returnMsg, result } = data
        if (result) {
          this.withdrawForm = {
            bank: "1",
            account: '',
            amount: ''
          }
        }
        alert(returnMsg)
      })
    },
    showConfirm() {
      const { registPoint } = this
      if (!registPoint || isNaN(Number(registPoint))) {
        alert('請輸入正確的轉入點數數量')
      }
      this.mm_2nd_pwd = ''
      $("#confirmModal").modal("show")
    },
    submitTransfer() {
      const { registPoint, registPointTarget } = this
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      const { mm_2nd_pwd } = this
      if (!mm_2nd_pwd) {
        alert('請輸入您的二級密碼')
        return
      }
      const url = registPointTarget? 'PointConversion/GiftPoint' : 'PointConversion'
      const body = {
        mm_id: userData.mm_id,
        amount: Number(registPoint),
        mm_2nd_pwd
      }
      if (registPointTarget) body.recipient = registPointTarget
      this.isLoading = true
      axios.post(baseUrl+url, body).then(({ data }) => {
        this.isLoading = false
        if (!data.result) {
          alert(data.returnMsg)
        } else {
          $("#confirmModal").modal("hide")
          this.registPoint = undefined
          this.registPointTarget = ''
          this.getWallet()
        }
      })
    },
    // 圖片上傳顯示處理
    imgFileReader(input, fileTarget) {
      const file = input.target.files[0];
      if (!file) {
        fileTarget.file = null
        fileTarget.base64 = ""
        return
      }
      const reader = new FileReader();
      reader.onload = function(e) {
        const img = new Image();
        img.src = e.target.result;
        if(img != "") {
          fileTarget.file = file
          fileTarget.base64 = img.src
        } else {
          fileTarget.file = null
          fileTarget.base64 = ""
        }
      };
      reader.readAsDataURL(file);
    },
    getConfig() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"PointConversion/"+userData.mm_id).then(({ data }) => {
          this.conf = data.result || {}
        })

      } else {
        location.href = "./login.html"
      }
    }
  },
  mounted() {
    this.pageGroupSize = window.innerWidth > 992? 8 : 3;
    $(".walletNav").addClass("active")
    const userData = JSON.parse(localStorage.getItem("mm_user_data"))
    if (userData && userData.mm_id && userData.accessToken) {
      this.getUserData()
      this.getWallet()
      this.getDepositData()
      this.getConfig()
    } else {
      location.href = "./login.html"
    }
    this.checkUrl()
  }
})