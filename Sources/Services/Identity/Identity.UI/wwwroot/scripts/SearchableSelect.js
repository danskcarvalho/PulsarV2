function InitializeSelectComponents(sel) {
    if (sel) {
        var v = $(sel).val();
        $(sel).selectpicker('refresh');
        $(sel).selectpicker('val', v);
    }
}