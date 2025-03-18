document.querySelectorAll('.clickable-screenshot').forEach(function(img) {
    img.addEventListener('click', function(){
        document.getElementById('modalImage').src = this.src;
        document.getElementById('modalImage').alt = this.alt;
    });
});