const app = new Vue({
  el: "#app",
  mixins: [headerControl],
  data() {
    return {
      banners: [],
      notice: [],
      ytVideos: [],
      news: {},
      activities: [],
      players: []
    }
  },
  methods: {
    getSwipper() {
      axios.get(baseUrl+"HomePicture/GetAnnImages").then(({ data }) => {
        this.banners = data.result
        const swiper = new Swiper(".homeSwiper", {
          slidesPerView: 1,
          spaceBetween: 10,
          pagination: {
            el: ".swiper-pagination",
            type: "bullets",
            clickable: true,
          },
          navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
          },
        });
      })
    },
    getNotices() {
      axios.get(baseUrl+"Announcement/QueryAnnBoardUser").then(({ data }) => {
        this.notice = data.result.gridRespResult
        const newsSwiper = new Swiper(".newsSwiper", {
          slidesPerView: 1.5,
          spaceBetween: 10,
          centeredSlides: true,
          pagination: {
            el: ".swiper-pagination",
            clickable: true,
          },
        })
      })
    },
    getActivityImages() {
      axios.get(baseUrl+"HomePicture/GetBlooperImages").then(({ data }) => {
        this.activities = data.result
        const systemSwiper = new Swiper(".systemSwiper", {
          slidesPerView: 1,
          spaceBetween: 10,
          pagination: {
            el: ".swiper-pagination",
            type: "fraction",
            clickable: false,
          },
          navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
          },
        });
      })
    },
    getVideos() {
      axios.get(baseUrl+"HomeSetting/GetHomeVideo").then(({ data }) => {
        const { result } = data
        const keys = Object.keys(result)
        keys.forEach(key => {
          if (key.match('video_url')) this.ytVideos.push(result[key])
        })
        this.initVideo()
      })
    },
    initVideo() {
      const myCarouselElement = document.querySelector('#ytCarousel')
      const carousel = new bootstrap.Carousel(myCarouselElement, {
        interval: 2000,
        touch: true
      })
      this.$nextTick(() => {
        this.ytVideos.forEach((el, ind) => {
          this.players.push(new YT.Player(`ytVideo${ind}`))
        })
      })
      const vm = this
      myCarouselElement.addEventListener('slide.bs.carousel', function (event) {
        // 暫停當前播放的影片
        const fromIndex = event.from;
        var player = vm.players[fromIndex];
        if (player && player.pauseVideo) {
          player.pauseVideo();
        }
      });
    },
    formatDate(dateString) {
      return dateString.substring(0, 10)
    },
    showNews(news) {
      this.news = news
      $("#newsDetailModal").modal("show")
    },
    test(idx) {
      const target = document.getElementById('yt-'+idx)
      target.style.pointerEvents = 'auto'
    }

  },
  mounted() {
    const tag = document.createElement('script');
    tag.src = "https://www.youtube.com/iframe_api";
    const firstScriptTag = document.getElementsByTagName('script')[0];
    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag)
    this.getSwipper()
    this.getNotices()
    this.getVideos()
    this.getActivityImages()
    $(".homeNav").addClass("active")
  }
})