document.addEventListener('DOMContentLoaded', function () {
    const entityName = 'Bolum';
    let isProcessing = false;

    const initialized = {
        delete: false,
        add: false,
        edit: false
    };

    if (!initialized.delete) {
        initializeDeleteButtons(entityName);
        initialized.delete = true;
    }

    if (!initialized.add) {
        initializeAddButton(entityName);
        initialized.add = true;
    }

    if (!initialized.edit) {
        initializeEditButtons(entityName);
        initialized.edit = true;
    }
});

// Satıra dokununca nesne silme
function initializeDeleteButtons(entityName) {
    document.querySelectorAll('.delete-btn').forEach(button => {
        button.replaceWith(button.cloneNode(true));
        
        document.querySelector(`button[data-id="${button.dataset.id}"]`).addEventListener('click', async function(e) {
            e.preventDefault();
            e.stopPropagation();

            if (isProcessing) return;
            isProcessing = true;

            try {
                const id = this.getAttribute('data-id');
                const name = this.getAttribute('data-name');

                const result = await Swal.fire({
                    title: 'Emin misiniz?',
                    text: `"${name}" bölümünü silmek istediğinize emin misiniz?`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    confirmButtonText: 'Evet, Sil!',
                    cancelButtonText: 'İptal'
                });

                if (result.isConfirmed) {
                    await deleteEntity(id, entityName);
                }
            } finally {
                isProcessing = false;
            }
        }, { once: true });
    });
}

// API işlemleri
function deleteEntity(id, entityName) {
    fetch(`/${entityName}/Delete`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Id: id })
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Sunucu yanıt vermedi');
        }
        return response.text().then(text => {
            try {
                return text ? JSON.parse(text) : {}
            } catch (e) {
                throw new Error('Geçersiz JSON yanıtı: ' + text);
            }
        });
    })
    .then(result => {
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 700,
                showConfirmButton: false
            }).then(() => {
                location.reload();
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Hata!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 3000,
                showConfirmButton: false
            });
        }
    })
    .catch(error => {
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Bir hata oluştu: ' + error,
            toast: true,
            position: 'top-end',
            timer: 3000,
            showConfirmButton: false
        });
    });
}

// Ekleme işlemi
function initializeAddButton(entityName) {
    const inputRow = document.getElementById("inputRow");
    const tableBody = document.getElementById("tableBody");
    
    // Ekle butonuna tıklandığında
    document.getElementById("addRowBtn").addEventListener("click", function () {
        // Input satırını göster ve en üste taşı
        inputRow.style.display = "table-row-group";
        // Input row'u tablonun en üstüne taşı
        tableBody.parentNode.insertBefore(inputRow, tableBody);
        // Input alanını temizle
        document.getElementById('newAd').value = '';
    });

    // İptal butonuna tıklandığında
    document.getElementById("cancelNewRowBtn").addEventListener("click", function() {
        // Input satırını gizle
        inputRow.style.display = "none";
        // Input alanını temizle
        document.getElementById('newAd').value = '';
        
        // İptal edildiğini bildir
        Swal.fire({
            icon: 'info',
            title: 'İptal Edildi',
            text: 'Ekleme işlemi iptal edildi.',
            toast: true,
            position: 'top-end',
            timer: 700,
            showConfirmButton: false
        });
    });

    // Kaydet butonuna tıklandığında
    document.getElementById("submitBtn").addEventListener("click", function() {
        const newAd = document.getElementById('newAd').value;
        if (!newAd) {
            Swal.fire({
                icon: 'warning',
                title: 'Uyarı!',
                text: 'Lütfen bölüm adını giriniz.',
                toast: true,
                position: 'top-end',
                timer: 3000,
                showConfirmButton: false
            });
            return;
        }
        addEntity(newAd, entityName);
    });
}

// Düzenleme işlemi
function initializeEditButtons(entityName) {
    document.querySelectorAll('.edit-btn').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            const currentName = this.getAttribute('data-name');

            Swal.fire({
                title: 'Bölüm Düzenle',
                input: 'text',
                inputValue: currentName,
                showCancelButton: true,
                confirmButtonText: 'Güncelle',
                cancelButtonText: 'İptal',
                inputValidator: (value) => {
                    if (!value) {
                        return 'Lütfen bir değer giriniz!';
                    }
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    updateEntity(id, result.value, entityName);
                }
            });
        });
    });
}

function addEntity(name, entityName) {
    fetch(`/${entityName}/Add`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Ad: name })
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Sunucu yanıt vermedi');
        }
        return response.text().then(text => {
            try {
                return text ? JSON.parse(text) : {}
            } catch (e) {
                throw new Error('Geçersiz JSON yanıtı: ' + text);
            }
        });
    })
    .then(result => {
        console.log(result);
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 700,
                showConfirmButton: false
            }).then(() => {
                location.reload();
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Hata!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 3000,
                showConfirmButton: false
            });
        }
    })
    .catch(error => {
        console.log(error);
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Bir hata oluştu: ' + error,
            toast: true,
            position: 'top-end',
            timer: 3000,
            showConfirmButton: false
        });
    });
}

function updateEntity(id, newName, entityName) {
    fetch(`/${entityName}/Update`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Id: id, Ad: newName })
    })
    .then(response => {
        if (!response.ok) {
            Swal.fire({
                icon: 'error',
                title: 'Hata!',
                text: 'Sunucu yanıt vermedi',
                toast: true,
                position: 'top-end',
                timer: 3000,
                showConfirmButton: false
            });
            return;
        }
        return response.text().then(text => {
            try {
                return text ? JSON.parse(text) : {}
            } catch (e) {
                Swal.fire({
                    icon: 'error',
                    title: 'Hata!', 
                    text: 'Geçersiz JSON yanıtı: ' + text,
                    toast: true,
                    position: 'top-end',
                    timer: 3000,
                    showConfirmButton: false
                });
                return;
            }
        });
    })
    .then(result => {
        console.log("result.success:", result.success);
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 700,
                showConfirmButton: false
            }).then(() => {
                location.reload();
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Hata!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 3000,
                showConfirmButton: false
            });
        }
    })
    .catch(error => {
        console.log(error);
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Bir hata oluştu: ' + error,
            toast: true,
            position: 'top-end',
            timer: 3000,
            showConfirmButton: false
        });
    });
}
