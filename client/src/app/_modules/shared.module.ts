import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ToastrModule } from 'ngx-toastr';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxGalleryModule } from '@kolkov/ngx-gallery';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    TabsModule.forRoot(),
    BsDropdownModule.forRoot(),
    ToastrModule.forRoot({
      timeOut: 2000,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
    }),
    NgxGalleryModule,
  ],
  exports: [BsDropdownModule, ToastrModule, TabsModule, NgxGalleryModule],
})
export class SharedModule {}
