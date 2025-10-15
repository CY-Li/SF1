import { Component, computed, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { CommonModule } from '@angular/common';

export interface MenuItem {
  icon?: string;
  label?: string;
  route?: string;
  subMenuOpen?: boolean;
  subMenuItem?: MenuItem[];
};

@Component({
  selector: 'app-manage',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatSidenavModule,
    MatListModule,
    RouterOutlet,
    RouterModule],
  templateUrl: './manage.component.html',
  styleUrl: './manage.component.scss'
})
export class ManageComponent {
  menuCollapsed = signal(false);
  sidenavWidth = computed(() => this.menuCollapsed() ? '0px' : '250px');

  toggleSubMenuNested(row: MenuItem) {
    if (row.subMenuItem) {
      row.subMenuOpen = !row.subMenuOpen;
    }
  }

  menuItems = signal<MenuItem[]>([
    {
      icon: 'settings_suggest',
      label: '系統管理',
      route: '',
      subMenuOpen: false,
      subMenuItem: [
        {
          icon: '',
          label: '參數設定',
          route: '/manage/Sps'
        },
        {
          icon: '',
          label: '公告',
          route: '/manage/AnnouncementBoard'
        },
      ],
    },
    {
      icon: 'image',
      label: '圖片管理',
      route: '',
      subMenuOpen: false,
      subMenuItem: [
        {
          icon: '',
          label: '公告圖片',
          route: '/manage/Announcement'
        },{
          icon: '',
          label: '花絮圖片',
          route: '/manage/BlooperImages'
        },
      ],
    },
    {
      icon: 'description',
      label: '標會管理',
      route: '',
      subMenuOpen: false,
      subMenuItem: [{
        icon: '',
        label: '標會',
        route: '/manage/Tender'
      },
      ],
    },
    {
      icon: 'account_circle',
      label: '會員管理',
      route: '',
      subMenuOpen: false,
      subMenuItem: [{
        icon: '',
        label: '會員資料',
        route: '/manage/Member'
      },
      {
        icon: '',
        label: '錢包資料',
        route: '/manage/Wallet'
      },
      ],
    },
    {
      icon: 'check_circle',
      label: '權限管理',
      route: '',
      subMenuOpen: false,
      subMenuItem: [{
        icon: '',
        label: 'KYC',
        route: '/manage/Kyc'
      },
      ],
    },
    {
      icon: 'account_balance',
      label: '帳務管理',
      route: '',
      subMenuOpen: false,
      subMenuItem: [{
        icon: '',
        label: '儲值',
        route: '/manage/ApplyDeposit'
      },
      {
        icon: '',
        label: '提領',
        route: '/manage/ApplyWithdraw'
      },
      {
        icon: '',
        label: '帳務紀錄',
        route: '/manage/MemberBalance'
      },
      ],
    },
    {
      icon: 'quiz',
      label: '測試',
      route: '',
      subMenuOpen: false,
      subMenuItem: [{
        icon: '',
        label: '排程',
        route: '/manage/SkdTest'
      },
      {
        icon: '',
        label: 'CRID測試',
        route: '/manage/CrudTest'
      },

      ],
    },
  ]);
}
