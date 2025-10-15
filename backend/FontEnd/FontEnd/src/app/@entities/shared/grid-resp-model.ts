export interface GridRespModel<T> {
    gridRespResult: T[]; // 使用泛型 GM 的陣列來表示 GridRESPResult
    gridTotalCount: number; // Grid 總筆數
}
