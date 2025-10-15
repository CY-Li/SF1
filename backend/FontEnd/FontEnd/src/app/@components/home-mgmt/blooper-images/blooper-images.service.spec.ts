import { TestBed } from '@angular/core/testing';

import { BlooperImagesService } from './blooper-images.service';

describe('BlooperImagesService', () => {
  let service: BlooperImagesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BlooperImagesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
