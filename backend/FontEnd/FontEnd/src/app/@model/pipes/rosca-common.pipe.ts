import { Pipe, PipeTransform } from '@angular/core';
import { ad_status_enum, aw_status_enum, mb_type_enum, roscam_status_enum, tm_type_enum } from '../enums/rosca-common-enum';

@Pipe({
  name: 'roscaCommon',
  standalone: true
})
export class RoscaCommonPipe implements PipeTransform {

  transform(value: number, filedName?: string): string {
    if (filedName === "tm_type") {
      return tm_type_enum[value] || "N/A";
    } else if (filedName === "roscam_status") {
      return roscam_status_enum[value] || "N/A";
    } else if (filedName === "ad_status") {
      return ad_status_enum[value] || "N/A"
    } else if (filedName === "aw_status") {
      return aw_status_enum[value] || "N/A"
    } else if (filedName === "mb_type") {
      return mb_type_enum[value] || "N/A"
    } else {
      return 'N/A';
    }
  }
}
