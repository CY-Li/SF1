import { TestBed } from '@angular/core/testing';

import { CustomizeValidatorService } from './customize-validator.service';

describe('CustomizeValidatorService', () => {
  let service: CustomizeValidatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomizeValidatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
