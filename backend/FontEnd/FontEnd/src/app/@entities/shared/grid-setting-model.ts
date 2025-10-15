export interface GridSettingModel {
    pageSize?: number;//頁面顯示筆數
    pageIndex?: number;//取得第幾頁
    preGetIndex?: number;//取得幾頁資料，0為全部
    pageSort?:string;//頁面的排序欄位
    pageSortDirection?:string;//頁面的排序順序
    sortColumn?: string;//排序欄位
}
