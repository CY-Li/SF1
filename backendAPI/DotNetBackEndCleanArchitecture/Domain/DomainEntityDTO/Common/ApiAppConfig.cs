using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Common
{
    public class ApiAppConfig
    {
        //基底APIUrl 於program使用
        public string? APIUrl { get; set; }
        public JwtSettings JwtSettings { get; set; } = new JwtSettings();

        public string? RootFolderPath { get; set; }
        public string? RootGuid { get; set; }
        public string? KycFolderPath { get; set; }
        public string? KycGuid { get; set; }
        public string? AnnFolderPath { get; set; }
        public string? AnnGuid { get; set; }
        public string? AnnBoardFolderPath { get; set; }
        public string? AnnBoardGuid { get; set; }
        public string? DepositFolderPath { get; set; }
        public string? DepositGuid { get; set; }
        public string? WithdrawrFolderPath { get; set; }
        public string? WithdrawrGuid { get; set; }
        public string? BlooperFolderPath { get; set; }
        public string? BlooperGuid { get; set; }
    }

    public class JwtSettings
    {
        public bool ValidateIssuer { get; set; } = true;
        public string Issuer { get; set; } = string.Empty;
        public bool ValidateAudience { get; set; } = true;
        public string Audience { get; set; } = string.Empty;
        public int Expires { get; set; } = 0;
        public bool ValidateIssuerSigningKey { get; set; } = true;
        public string SecurityKey { get; set; } = string.Empty;
        public bool ValidateLifetime { get; set; } = true;
    }
}
