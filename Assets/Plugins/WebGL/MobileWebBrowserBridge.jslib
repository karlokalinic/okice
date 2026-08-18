mergeInto(LibraryManager.library, {
  Karlolegend_SetInputMode: function (mode) {
    if (typeof window !== 'undefined' && typeof window.KARLOLEGEND_SetInputMode === 'function') {
      window.KARLOLEGEND_SetInputMode(mode | 0);
    }
  }
});
