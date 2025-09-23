<script>
        // Ekle butonuna tıklanınca input satırını göster
    document.getElementById("addRowBtn").addEventListener("click", function() {
        document.getElementById("inputRow").style.display = "table-row";
        });

    // Submit butonuna tıklanınca veri gönder
    document.getElementById("submitBtn").addEventListener("click", function() {
            var sectionName = document.getElementById("sectionName").value;

    if (sectionName) {
        fetch('/Home/YeniBolumEkle', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(sectionName)
        })
            .then(response => response.json())
            .then(data => {
                // Yeni veri eklendikten sonra inputu temizle ve gizle
                document.getElementById("sectionName").value = '';
                document.getElementById("inputRow").style.display = "none";
                alert("Bölüm başarıyla eklendi!");
            });
            }
        });
</script>