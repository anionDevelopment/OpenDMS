import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class UtilitiesService {

  constructor() { }

  getURLWithoutQueryParameter(router: Router) {
    let urlTree = router.parseUrl(router.url);
    urlTree.queryParams = {};
    urlTree.fragment = null; // optional
    return urlTree.toString();
  }
}
