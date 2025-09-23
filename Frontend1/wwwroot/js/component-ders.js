document.addEventListener('DOMContentLoaded', function () {
    const entityName = 'Ders';
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
                    text: `"${name}" dersini silmek istediğinize emin misiniz?`,
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

async function deleteEntity(id, entityName) {
    if (isProcessing) return;
    isProcessing = true;

    try {
        const response = await fetch(`/${entityName}/Delete/${id}`, {
            method: 'POST'
        });

        const result = await response.json();

        if (result.success) {
            await Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 2000,
                showConfirmButton: false
            });
            document.querySelector(`tr[data-id="${id}"]`).remove();
        } else {
            await Swal.fire({
                icon: 'error',
                title: 'Hata!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 3000,
                showConfirmButton: false
            });
        }
    } catch (error) {
        await Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Bir hata oluştu: ' + error,
            toast: true,
            position: 'top-end',
            timer: 3000,
            showConfirmButton: false
        });
    } finally {
        isProcessing = false;
    }
}

function initializeAddButton(entityName) {
    const inputRow = document.getElementById("inputRow");
    const tableBody = document.getElementById("tableBody");
    let addButtonInitialized = false;
    
    if (!addButtonInitialized) {
        document.getElementById("addRowBtn")?.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            
            if (isProcessing) return;
            isProcessing = true;

            try {
                inputRow.style.display = "table-row-group";
                tableBody.parentNode.insertBefore(inputRow, tableBody);
                document.getElementById('newAd').value = '';
            } finally {
                isProcessing = false;
            }
        }, { once: true });

        document.getElementById("cancelNewRowBtn")?.addEventListener("click", async function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            if (isProcessing) return;
            isProcessing = true;

            try {
                inputRow.style.display = "none";
                document.getElementById('newAd').value = '';
                
                await Swal.fire({
                    icon: 'info',
                    title: 'İptal Edildi',
                    text: 'Ekleme işlemi iptal edildi.',
                    toast: true,
                    position: 'top-end',
                    timer: 2000,
                    showConfirmButton: false
                });
            } finally {
                isProcessing = false;
            }
        }, { once: true });

        document.getElementById("submitBtn")?.addEventListener("click", async function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            if (isProcessing) return;
            isProcessing = true;

            try {
                const newAd = document.getElementById('newAd').value;
                if (!newAd) {
                    await Swal.fire({
                        icon: 'warning',
                        title: 'Uyarı!',
                        text: 'Lütfen ders adını giriniz.',
                        toast: true,
                        position: 'top-end',
                        timer: 3000,
                        showConfirmButton: false
                    });
                    return;
                }
                await addEntity(newAd, entityName);
            } finally {
                isProcessing = false;
            }
        }, { once: true });

        addButtonInitialized = true;
    }
}

// Düzenleme işlemi
function initializeEditButtons(entityName) {
    document.querySelectorAll('.edit-btn').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            const currentName = this.getAttribute('data-name');

            Swal.fire({
                title: 'Ders Düzenle',
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
    .then(response => response.json())
    .then(result => {
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 2000,
                showConfirmButton: false
            });
            location.reload();
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

function updateEntity(id, newName, entityName) {
    fetch(`/${entityName}/Update`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Id: id, Ad: newName })
    })
    .then(response => response.json())
    .then(result => {
        if (result.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: result.message,
                toast: true,
                position: 'top-end',
                timer: 2000,
                showConfirmButton: false
            });
            location.reload();
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
