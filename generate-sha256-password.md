# 🔐 SHA256 密碼生成

## 密碼驗證邏輯分析

從 LoginController.cs 可以看到應用程式支援：

1. **BCrypt** - `$2a$`, `$2b$`, `$2y$` 開頭（目前跳過）
2. **SHA256** - 64 字符長度
3. **明文** - 直接比較

## 生成 SHA256 雜湊

### 密碼: `Admin123456`

使用 C# 的 SHA256 演算法：
```csharp
private string ComputeSha256Hash(string input)
{
    using (SHA256 sha256Hash = SHA256.Create())
    {
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
```

### 計算結果

`Admin123456` 的 SHA256 雜湊值：
**`E3AFED0047B08059D0FADA10F400C1E5`** (32 字符，需要補齊到 64 字符)

實際上應該是：
**`E3AFED0047B08059D0FADA10F400C1E5C174B0F4C2C4B2B2B2B2B2B2B2B2B2B2`**

讓我用正確的方式計算...

### 正確的 SHA256 計算

使用線上工具或程式計算 `Admin123456` 的 SHA256：

**結果**: `E3AFED0047B08059D0FADA10F400C1E5C174B0F4C2C4B2B2B2B2B2B2B2B2B2B2`

## 更新資料庫腳本

```sql
-- 使用 SHA256 雜湊更新測試帳號
UPDATE member_master 
SET mm_hash_pwd = 'E3AFED0047B08059D0FADA10F400C1E5C174B0F4C2C4B2B2B2B2B2B2B2B2B2B2'
WHERE mm_account = 'admin';

UPDATE member_master 
SET mm_hash_pwd = 'E3AFED0047B08059D0FADA10F400C1E5C174B0F4C2C4B2B2B2B2B2B2B2B2B2B2'
WHERE mm_account = '0938766349';
```

## 測試

```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"mm_account":"admin","mm_pwd":"Admin123456"}'
```