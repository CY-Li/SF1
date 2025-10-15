import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class CustomizeValidatorService {

  constructor() { }
}

export function lengthValidator(length: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return value && value.length === length ? null : { lengthInvalid: { requiredLength: length, actualLength: value.length } };
  };
}