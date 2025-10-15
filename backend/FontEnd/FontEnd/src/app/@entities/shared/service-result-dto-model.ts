export interface ServiceResultDtoModel<T> {
    result?: T;
    returnStatus: number;
    returnMsg: string;
}
