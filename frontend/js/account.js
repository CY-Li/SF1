$(function() {
  // 選單切換效果
  $("#footer").load("./component/footer.html")

  // 切換參與/未參與標案
  $(".tenderBtn").click(function() {
    $(".tenderBtn").removeClass("active");
    $(this).addClass("active");
  })

  $(".btn.cancel").click(function() {
    $(".addTableRow").hide();
  })

  $(".toggleCoumn-md .toggleBtn").click(function() {
    $(this).parent(".toggleCoumn-md").siblings(".positionColumn").show();
  })


})

Vue.component('group-chip', {
  props: ['item', 'key'],
  template: `
    <div class="d-flex justify-content-center flex-wrap">
      <div class="dragItem" :class="{'active': !item.hideChildren}" @click="item.hideChildren = !item.hideChildren">
        <div class="w-100 mb-1" >F{{ item.mm_level }} {{item.mm_name}}({{item.mw_subscripts_count}})</div>
        <div class="w-100" >{{item.mm_account}}({{item.total_qty}})</div>
        <div v-if="key === 0" class="star"><i class="bi bi-star-fill"></i></div>
      </div>
      <div class="dragItems children" v-if="item.items && item.items.length" v-show="!item.hideChildren">
        <group-chip v-for="child in item.items" :item="child" :key="child.mm_account" />
      </div>
    </div>

  `
});

const app = new Vue({
  el: "#app",
  mixins: [headerControl],
  data() {
    return {
      userData: {},
      newPswd: "",
      newPswdConfirm: "",
      mm_2nd_pwd: '',
      mm_2nd_pwdConfirm: '',
      uploads: [
        {
          title: "本人身份證之正反面",
          subtitle: "(身分證或護照)",
          images: [
            { label: "正面", file: null, base64: '' },
            { label: "反面", file: null, base64: '' },
          ]
        },
      ],
      currrentPage: 1,
      groups: [],
      referee: '',
      kycData: {
        userName: '',
        gender: 0,
        id: '',
        address: '',
        email: '',
        bank_code: '',
        account: '',
        bankUserName: '',
        bankBranch: '',
        wallet_address: '',
        beneficiary: '',
        beneficiary_phone: '',
        relation: '',
      },
      tenderTabType: 1,
      allTenders: [],
      attendTenders: [],
      tenderCount: {},
      tenderDetail: {}
    }
  },
  computed: {
    tenders() {
      const { tenderTabType, allTenders, attendTenders } = this
      return tenderTabType == 1? allTenders : attendTenders
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
            const url = location.origin
            this.referee = `${url}/login.html?referee=${result.mm_account}`
          } else {
            alert(returnMsg)
          }
        })

      } else {
        location.href = "./login.html"
      }
    },
    updateUserData() {
      const { newPswd, newPswdConfirm, mm_2nd_pwd, mm_2nd_pwdConfirm } = this
      const body = {}
      if (newPswd && newPswdConfirm !== newPswd) {
        alert("密碼輸入錯誤")
        return
      } else {
        body.mm_pwd = newPswd? newPswd : ""
      }

      if (mm_2nd_pwd && mm_2nd_pwd !== mm_2nd_pwdConfirm) {
        alert("二級密碼輸入錯誤")
        return
      } else {
        body.mm_2nd_pwd = mm_2nd_pwd? mm_2nd_pwd : ""
      }
      axios.put(baseUrl+"Members/Member", body).then(({ data }) => {
        const { returnStatus, result, returnMsg } = data
        if (returnStatus === 1) {
          this.userData = result
          // 更新localStorage
          const mm_data = JSON.parse(localStorage.getItem("mm_user_data"))
          mm_data.mm_name = body.mm_name
          localStorage.setItem("mm_user_data", JSON.stringify(mm_data))
          location.reload()
        } else {
          alert(returnMsg)
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
          // vm.resizeImage(vm.base64Img)
        } else {
          fileTarget.file = null
          fileTarget.base64 = ""
        }
      };
      reader.readAsDataURL(file);
    },
    resizeImage(image) {
      // const vm = this;
      // let nextQ;
      // vm.uploadFile = $("#uploadFile")[0].files[0];
      // var base64Codes;
      // image.onload = function () {
      //   if (image.width > vm.maxWidth || image.height > vm.maxHeight) {
      //     if (image.width > image.height) {
      //       nextQ = vm.maxWidth / image.width;
      //     } else {
      //       nextQ = vm.maxHeight / image.height;
      //     }
      //     var canvas = document.createElement('canvas');
      //     var ctx = canvas.getContext('2d');
      //     ctx.clearRect(0, 0, canvas.width, canvas.height);
      //     canvas.width = image.width * nextQ;
      //     canvas.height = image.height * nextQ;
      //     ctx.drawImage(image, 0, 0, canvas.width, canvas.height);
      //     base64Codes = canvas.toDataURL(uploadFile.type, nextQ); 
      //     vm.uploadFile = vm.dataUrlToFile(base64Codes, "uploadFile.name.split('.')[0]"); 
      //   }
      // }
    },
    formatDate(dateString) {
      if (!dateString) return '-'
      return dateString.substring(0, 10)
    },
    toggleNav(target) {
      $(".nav").removeClass("active");
      $(`.nav.${target}`).addClass("active");
      if (target == "subaccount" || target == "invite") {
        $(".childNav").addClass("active")
      } else {
        $(".mainNav").addClass("active")
      }
    },
    toggleForm(target) {
      $(`.frame`).addClass("d-none");
      $(`.frame.${target}`).removeClass("d-none");
      if (target == "subaccount") {
        $(".accountListTable").removeClass("d-none");
        this.defineDetailPagination(window.innerWidth > 992? 8 : 3);
      } else if (target == "invite") {
        $(".inviteTable").removeClass("d-none");
      }
    },
    defineDetailPagination(pageNum) {
      const url = new URL(window.location);
      let currrentPage = url.searchParams.get("page") || 1; // 當前資料頁面
      currrentPage = Number(currrentPage);
      let pageGroup = currrentPage; // 分頁顯示的群組
      const totalPage = $(".accountListTable .page").length;
      const group = Math.ceil(pageGroup / pageNum);
      // 往前/後切頁按鈕
      if (currrentPage == 1) {
        $(".accountListTable .page-prev").addClass("disabled");
      } else if (currrentPage == totalPage) {
        $(".accountListTable .page-next").addClass("disabled");
      }
      // 切換頁籤群組按鈕
      $(".accountListTable .page-ellipsis-prev")[group == 1? "addClass" : "removeClass"]("d-none");
      $(".accountListTable .page-ellipsis-next")[group * pageNum >= totalPage? "addClass" : "removeClass"]("d-none");
      
      // 分頁是否顯示
      $(".accountListTable .page").each(function() {
        const page = $(this).data("page");
        if (page == currrentPage) $(this).addClass("active")
        if ((page > (pageNum * (group - 1)) && page <= pageNum * group)) {
          $(this).removeClass("d-none")
        } else {
          $(this).addClass("d-none")
        }
      })
    },
    copyText() {
      const inviteLinkElement = document.getElementById('invite_link');
      const texts = inviteLinkElement ? inviteLinkElement.value : '';
      if (texts.trim() === '') {
        console.error("No text to copy!");
        return;
      }
      if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(texts)
          .then(() => {
            console.log("Text copied to clipboard successfully!")
          })
          .catch(err => {
            console.error("Failed to copy text: ", err)
          });
      } else {
        // 回退到 execCommand 方法
        inviteLinkElement.disabled = false
        inviteLinkElement.select()
        try {
          document.execCommand('copy');
          console.log("Text copied to clipboard (fallback method)!")
        } catch (err) {
          console.error("Failed to copy text using execCommand: ", err)
        }
        inviteLinkElement.disabled = true
      }
    },
    getGroup() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        this.groups = []
        axios.get(baseUrl+"Members/GetSubordinateOrg/"+userData.mm_id).then(({ data }) => {
          this.sortGroup(data.result)
        })

      }
    },
    sortGroup(groups) {
      const firstLevel = groups[0].class_level
      groups[0].mm_invite_account = ''
      const map = {}
      groups.forEach(item => {
        if (item.class_level > firstLevel) item.hideChildren = true
        map[item.mm_account] = { ...item, items: [] };
      })
      groups.forEach(item => {
        if (item.mm_invite_account) {
          if (map[item.mm_invite_account]) {
            map[item.mm_invite_account].items.push(map[item.mm_account]);
          }
        } else {
          this.groups.push(map[item.mm_account]);
        }
      })
    },
    groupLevel(ind) {
      return ind >= 2? 'level-3' : `level-${ind + 1}`
    },
    clickGroup(parentKey, ind) {
      if (parentKey > 0 && parentKey < this.groups.length - 1) {
        const targetClass = `.dragItem${parentKey}-${ind}`
        $(targetClass).toggleClass("active")
      }
    },
    logout() {
      localStorage.removeItem("mm_user_data")
      window.location.href = "./index.html"
    },
    getTenderCount() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"Tender/GetParticipationRecord?mm_id=" + userData.mm_id).then(({ data }) => {
          this.tenderCount = data.result
        })

      } else {
        location.href = "./login.html"
      }
    },
    getAttendTender() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"Tender/QueryTenderParticipatedUser?mm_id=" + userData.mm_id).then(({ data }) => {
          this.attendTenders = data.result.gridRespResult
        })

      } else {
        location.href = "./login.html"
      }
    },
    getAllTender() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"Tender/QueryTenderAllUser?mm_id=" + userData.mm_id).then(({ data }) => {
          this.allTenders = data.result.gridRespResult
        })

      } else {
        location.href = "./login.html"
      }
    },
    kycSubmit() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      const { gender, id, address, email, bank_code, account, bankUserName, bankBranch, userName, beneficiary, beneficiary_phone, relation, wallet_address } = this.kycData
      const front = this.uploads[0].images[0].base64
      const back = this.uploads[0].images[1].base64
      if (!userName) {
        alert("請選擇您的姓名")
        return
      }
      if (!gender) {
        alert("請選擇您的性別")
        return
      } else if (!front || !back) {
        alert("請上傳您的身分證正反面")
        return
      }
      const formData = new FormData()
      formData.append("akc_name", userName)
      formData.append("akc_gender", gender)
      formData.append("akc_mm_id", userData.mm_id)
      formData.append("akc_personal_id", id)
      formData.append("akc_address", address)
      formData.append("akc_mw_address", wallet_address)
      formData.append("akc_email", email)
      formData.append("akc_bank_code", bank_code)
      formData.append("akc_bank_account", account)
      formData.append("akc_bank_account_name", bankUserName)
      formData.append("akc_branch", bankBranch)
      formData.append("akc_front_image", this.uploads[0].images[0].file)
      formData.append("akc_back_image", this.uploads[0].images[1].file)
      formData.append("akc_id", 0)
      formData.append("akc_beneficiary_name", beneficiary)
      formData.append("akc_beneficiary_phone", beneficiary_phone)
      formData.append("akc_beneficiary_relationship", relation)

      axios.post(baseUrl+"Kyc", formData, { headers: { 'Content-Type': 'multipart/form-data' }}).then(({ data }) => {
        const { returnStatus, result, returnMsg } = data
        if (returnStatus === 1) {
          alert("已成功送出")
        } else {
          alert(returnMsg)
        }
      })
    },
    openDetail(tender) {
      axios.get(baseUrl+"Tender/GetTenderRecord?ttr_tm_sn=" + tender.tm_sn).then(({ data }) => {
        this.tenderDetail = data.result
        const arr = JSON.parse(this.tenderDetail.ttr_detail)
        const mid = Math.ceil(arr.length / 2)
        const firstHalf = arr.slice(0, mid)
        const secondHalf = arr.slice(mid)
        this.tenderDetail.detailRows = firstHalf.map((val, i) => secondHalf[i] !== undefined ? [val, secondHalf[i]] : [val])
        $("#recordModal").modal("show")
      })
    },
    formatDate(dateString) {
      if (!dateString) return '-'
      return dateString.substring(0, 10)
    },
    formatName(name) {
      const length = name?.length;
      if (length === 2) {
          return name[0] + "O";
      } else if (length === 3) {
        return name[0] + "O" + name[2];
      } else if (length >= 4) {
        return name[0] + "OO" + name[length - 1];
      }
      return name;
    },
    formatPhone(phone) {
      if (!phone) return ''
      const length = phone.length;
      if (length <= 3) return phone;
      const midIndex = Math.floor(length / 2) - 1; // 找到中間開始的位置
      return phone.slice(0, midIndex) + "XXX" + phone.slice(midIndex + 3)
    },
    formatAddress(address) {
      if (!address) return ''
      return address.slice(0, 3) + "XXX"
    },
    printData() {
      const printArea = document.getElementById("print-area").innerHTML;
      const printWindow = window.open("", "", "width=800,height=600");
      printWindow.document.write(`
        <html>
          <head>
            <title>列印</title>
            <link id="print-style" rel="stylesheet" href="./scss/account.css" />
            <style>
              @media print {
                .modal-footer { display: none; } /* 隱藏列印按鈕 */
              }
            </style>
          </head>
          <body>
            <div>${printArea}</div>
          </body>
        </html>
      `);
  
      printWindow.document.close();
      const link = printWindow.document.getElementById("print-style");
      link.onload = () => printWindow.print();
    }
  },
  mounted() {
    const url = new URL(window.location);
    const query = url.searchParams.get("type");
    const navQuerys = ["tender", "subaccount", "invite", 'kyc']
    if (navQuerys.indexOf(query) >= 0) {
      this.toggleNav(query);
      this.toggleForm(query);
    }
    $(".mainNav").addClass("active")

    this.getUserData()
    if (query == "invite") this.getGroup()
    if (query == 'tender') {
      this.getAttendTender()
      this.getAllTender()
      this.getTenderCount()
    }

    const child = document.querySelector(".dragBody")
    const panzoom = Panzoom(child, { canvas: true })
    zoomIn.addEventListener('click', panzoom.zoomIn)
    zoomOut.addEventListener('click', panzoom.zoomOut)
  }
})