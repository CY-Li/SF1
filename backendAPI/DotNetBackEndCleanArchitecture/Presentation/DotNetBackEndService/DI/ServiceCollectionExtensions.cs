using AppAbstraction.AuthenticationMgmt;
using AppAbstraction.FinancialMgmt;
using AppAbstraction.MemberMgmt;
using AppAbstraction.SystemMgmt;
using AppAbstraction.TenderMgmt;
using AppService.AuthenticationMgmt;
using AppService.FinancialMgmt;
using AppService.MemberMgmt;
using AppService.SystemMgmt;
using AppService.TenderMgmt;
using DomainAbstraction.Interface.AuthenticationMgmt;
using DomainAbstraction.Interface.FinancialMgmt;
using DomainAbstraction.Interface.MemberMgmt;
using DomainAbstraction.Interface.SystemMgmt;
using DomainAbstraction.Interface.TenderMgmt;
using DotNetBackEndService.Controllers.FinancialMgmt;
using InfraCommon.Common;
using InfraCommon.DBA;
using Persistence.Impl.AuthenticationMgmt;
using Persistence.Impl.FinancialMgmt;
using Persistence.Impl.MemberMgmt;
using Persistence.Impl.SystemMgmt;
using Persistence.Impl.TenderMgmt;
using Persistence.Schedule;

namespace DotNetBackEndService.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfraCommon(this IServiceCollection services)
        {
            //builder.Services.AddScoped<IDBAccess, DBAccess>(); // DapperAccess
            services.AddScoped<IDBAccess, MysqlDBAccess>(); // DapperAccess
            services.AddScoped<ICryptography, AesProcess>();
        }
        public static void AddAuthenticationMgmtServices(this IServiceCollection services)
        {
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ILoginRepository, LoginRepository>();

            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IRegisterRepository, RegisterRepository>();

            services.AddScoped<IKycService, KycService>();
            services.AddScoped<IKycRepository, KycRepository>();

        }

        public static void AddFinancialMgmtServices(this IServiceCollection services)
        {
            services.AddScoped<IMemberBalanceService, MemberBalanceService>();
            services.AddScoped<IMemberBalanceRepository, MemberBalanceRepository>();

            services.AddScoped<IApplyDepositService, ApplyDepositService>();
            services.AddScoped<IApplyDepositRepository, ApplyDepositRepository>();

            services.AddScoped<IApplyWithdrawService, ApplyWithdrawService>();
            services.AddScoped<IApplyWithdrawRepository, ApplyWithdrawRepository>();

            services.AddScoped<IPointConversionService, PointConversionService>();
            services.AddScoped<IPointConversionRepository, PointConversionRepository>();
        }

        public static void AddMemberMgmtServices(this IServiceCollection services)
        {
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IMemberRepository, MemberRepository>();

            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IWalletRepository, WalletRepository>();
        }

        public static void AddSystemMgmtServices(this IServiceCollection services)
        {
            services.AddScoped<ISpsService, SpsService>();
            services.AddScoped<ISpsRepository, SpsRepository>();

            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        }

        public static void AddTenderMgmtServices(this IServiceCollection services)
        {
            services.AddScoped<ITenderService, TenderService>();
            services.AddScoped<ITenderRepository, TenderRepository>();
            services.AddScoped<IBiddingRepository, BiddingRepository>();
            services.AddScoped<IWinningTenderRepository, WinningTenderRepository>();
        }

        public static void AddScheduleMgmtServices(this IServiceCollection services)
        {
            services.AddTransient<BiddingSchedule>();
            services.AddTransient<SettlementRewardSchedule>();
            services.AddTransient<SettlementPeaceSchedule>();
            services.AddTransient<GroupDebitSchedule>();
            services.AddTransient<PendingPaymentSchedule>();
        }
    }
}
