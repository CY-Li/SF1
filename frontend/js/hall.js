const app = new Vue({
  el: "#app",
  mixins: [headerControl],
  data() {
    return {
      userData: {},
      searchCases: {
        pageSize: 0, // (必填，不填會一直取第一頁)一頁有幾筆，但是先統一，一頁10筆
        pageIndex: 0, // (必填，不填會一直取第一頁)第幾頁
        preGetIndex: 0, // 0代表全取
        specPageSize: 1, // (這個是有些特殊的取法，你填1就好)
      },
      caseType: { label: '一般玩法(散會)', value: 'A', name: '散會' },
      caseTypes: [
        { label: '一般玩法(散會)', value: 'A', name: '散會' },
        { label: '4人玩法(六會)', value: 'B', name: '六會' },
        { label: '2人玩法(十二會)', value: 'C', name: '十二會' },
        { label: '1人玩法(二十四會)', value: 'D', name: '二十四會' },
      ],
      caseList1: [], // 未成組的
      caseList2: [], // 已成組的
      subscriptItem: {},
      userParams: {
        id: "",
        token: "",
        account: ""
      },
      mm_2nd_pwd: '',
      tenderTabType: 1,
      allTenders: [],
      attendTenders: [],
      tenderDetail: {}
    }
  },
  computed: {
    filteredCase1() {
      const { caseType, caseList1 } = this
      const result = caseList1.filter(item => item.tm_type == caseType.value)
      return result
    },
    tenders() {
      const { tenderTabType, allTenders, attendTenders } = this
      return tenderTabType == 1? allTenders : attendTenders
    }
  },
  methods: {
    getTender1() {
      const { id } = this.userParams
      axios.get(baseUrl+"Tender", { params: { mm_id : id } }).then(({ data }) => {
        data.result.forEach(item => {
          item.qty = item.tm_type == 'D'? 1 : 0
        })
        this.caseList1 = data.result
      })
    },
    getTender2() {
      const { id } = this.userParams
      axios.get(baseUrl+"Tender/QueryTenderInProgressUser", { params: { mm_id : id, preGetIndex: 0 } }).then(({ data }) => {
        this.caseList2 = data.result.gridRespResult
      })
    },
    getUserData() {
      axios.get(baseUrl+"Members/"+this.userParams.id).then(({ data }) => {
        const { returnStatus, result, returnMsg } = data
        if (returnStatus === 1) {
          this.userData = result
        } else {
          alert(returnMsg)
        }
      })
    },
    showConfirm(item) {
      if (item.qty < 1) {
        alert("請選擇下標數量")
        return
      }
      this.subscriptItem = item
      this.mm_2nd_pwd = ''
      $("#confirmModal").modal("show")
    },
    subscriptCase() {
      const { mm_2nd_pwd } = this;
      if (!mm_2nd_pwd) {
        alert('請輸入您的二級密碼')
        return
      }
      const { subscriptItem, userParams } = this
      const { tm_id, qty } = subscriptItem
      const body = {
        mm_id: userParams.id,
        tm_id,
        tm_count: Number(qty),
        mm_2nd_pwd
      }
      axios.post(baseUrl+"Tender/Bidding", body).then(({ data }) => {
        $("#confirmModal").modal("hide")
        const { result, returnMsg } = data
        this.getTender1()
        // this.getTender2()
        this.getAttendTenders()
        if (!result) alert(returnMsg)
      })
    },
    getCaseCount(caseType) {
      return caseType == '1'? 24 : caseType == '3'? 2 : 4
    },
    getAttendTenders() {
      const userData = JSON.parse(localStorage.getItem("mm_user_data"))
      if (userData && userData.mm_id && userData.accessToken) {
        axios.get(baseUrl+"Tender/QueryTenderAllUser?mm_id=" + userData.mm_id).then(({ data }) => {
          this.allTenders = data.result.gridRespResult
        })
        axios.get(baseUrl+"Tender/QueryTenderParticipatedUser?mm_id=" + userData.mm_id).then(({ data }) => {
          this.attendTenders = data.result.gridRespResult
        })

      } else {
        location.href = "./login.html"
      }
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
            <link id="print-style" rel="stylesheet" href="./scss/hall.css" />
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
    const userData = JSON.parse(localStorage.getItem("mm_user_data"))
    if (userData && userData.mm_id && userData.accessToken && userData.mm_account) {
      $(".hallNav").addClass("active")
      this.userParams = {
        id: userData.mm_id,
        account: userData.mm_account,
        token: userData.accessToken,
      }
      this.getUserData()
      this.getTender1()
      // this.getTender2()
      this.getAttendTenders()
    } else {
      location.href = "./login.html"
    }
  }
})