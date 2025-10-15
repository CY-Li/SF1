import { Routes } from '@angular/router';
import { ManageComponent } from './@shared/manage/manage.component';
import { TenderComponent } from './@components/tender-mgmt/tender/tender.component';
import { MemberComponent } from './@components/member-mgmt/member/member.component';
import { ApplyDepositComponent } from './@components/member-mgmt/apply-deposit/apply-deposit.component';
import { MemberBalanceComponent } from './@components/member-mgmt/member-balance/member-balance.component';
import { SkdTestComponent } from './@components/skd-test/skd-test.component';
import { AnnouncementComponent } from './@components/system-mgmt/announcement/announcement.component';
import { SpsComponent } from './@components/system-mgmt/sps/sps.component';
import { KycComponent } from './@components/authn-mgmt/kyc/kyc.component';
import { ApplyWithdrawComponent } from './@components/member-mgmt/apply-withdraw/apply-withdraw.component';
import { AnnouncementBoardComponent } from './@components/system-mgmt/announcement-board/announcement-board.component';
import { CrudTestComponent } from './@components/system-mgmt/crud-test/crud-test.component';
import { WalletComponent } from './@components/member-mgmt/wallet/wallet.component';
import { BlooperImagesComponent } from './@components/home-mgmt/blooper-images/blooper-images.component';


export const routes: Routes = [
    { path: '', component: ManageComponent },
    {
        path: 'manage',
        component: ManageComponent,
        children: [
            { path: 'Tender', component: TenderComponent },
            { path: 'Member', component: MemberComponent },
            { path: 'Wallet', component: WalletComponent },
            { path: 'ApplyDeposit', component: ApplyDepositComponent },
            { path: 'ApplyWithdraw', component: ApplyWithdrawComponent },
            { path: 'MemberBalance', component: MemberBalanceComponent },
            { path: 'Kyc', component: KycComponent },
            { path: 'Sps', component: SpsComponent },
            { path: 'AnnouncementBoard', component: AnnouncementBoardComponent },
            { path: 'Announcement', component: AnnouncementComponent },
            { path: 'BlooperImages', component: BlooperImagesComponent },
            { path: 'SkdTest', component: SkdTestComponent },
            { path: 'CrudTest', component: CrudTestComponent },
            // { path: 'MemberInfo', component: MemberInfoComponent, canActivate: [authGuard] },
            // { path: '', component: NotfoundComponent }
        ]
    }

];
