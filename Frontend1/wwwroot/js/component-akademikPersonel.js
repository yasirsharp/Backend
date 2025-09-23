document.addEventListener('DOMContentLoaded', function () {
    const entityName = 'AkademikPersonel';
    initializeEventListeners(entityName);
});

function initializeEventListeners(entityName) {
    document.getElementById('addRowBtn').addEventListener('click', showAddRow);
    document.getElementById('cancelNewRowBtn').addEventListener('click', hideAddRow);
    document.getElementById('submitBtn').addEventListener('click', () => addEntity(entityName));
    updateDeleteButtons(entityName);
    updateEditButtons(entityName);
}

function showAddRow() {
    document.getElementById('inputRow').style.display = 'table-row-group';
    document.getElementById('newAd').value = '';
    document.getElementById('newUnvan').value = '';
}

function hideAddRow() {
    document.getElementById('inputRow').style.display = 'none';
    Swal.fire({
        icon: 'info',
        title: 'İptal Edildi',
        text: 'Ekleme işlemi iptal edildi.',
        toast: true,
        position: 'top-end',
        timer: 2000,
        showConfirmButton: false
    });
}

function addEntity(entityName) {
    const ad = document.getElementById('newAd').value.trim();
    const unvan = document.getElementById('newUnvan').value.trim();

    if (!ad || !unvan) {
        Swal.fire({
            icon: 'warning',
            title: 'Uyarı!',
            text: 'Lütfen tüm alanları doldurunuz.',
            toast: true,
            position: 'top-end',
            timer: 3000,
            showConfirmButton: false
        });
        return;
    }

    fetch(`/${entityName}/Add`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Ad: ad, Unvan: unvan })
    })
        .then(response => response.json())
        .then(result => handleResponse(result, 'Ekleme başarılı!'))
        .catch(handleError);
}

function updateDeleteButtons(entityName) {
    document.querySelectorAll('.delete-btn').forEach(button => {
        button.addEventListener('click', (event) => {
            event.stopPropagation(); // Satırın tıklanmasını engeller
            confirmDelete(button, entityName);
        });
    });
}

function confirmDelete(button, entityName) {
    const akademikPersonel = {
        id: button.getAttribute('data-id'),
        ad: button.getAttribute('data-name'),
        unvan: button.getAttribute('data-title'),
        userId: button.getAttribute('data-userId')
    };
    console.log(akademikPersonel);

    Swal.fire({
        title: 'Emin misiniz?',
        text: `"(${akademikPersonel.userId}) ${akademikPersonel.unvan} ${akademikPersonel.ad}" personelini silmek istediğinize emin misiniz?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Evet, Sil!',
        cancelButtonText: 'İptal'
    }).then(result => {
        if (result.isConfirmed) deleteEntity(akademikPersonel, entityName);
    });
}

function deleteEntity(akademikPersonel, entityName) {
    fetch(`/${entityName}/Delete`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(akademikPersonel)
    })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                document.querySelector(`tr[data-id="${akademikPersonel.id}"]`).remove();
            }
            handleResponse(result, 'Silme başarılı!');
        })
        .catch(handleError);
}

function updateEditButtons(entityName) {
    document.querySelectorAll('.edit-btn').forEach(button => {
        button.addEventListener('click', (event) => {
            event.stopPropagation(); // Satırın tıklanmasını engeller
            editEntity(button, entityName);
        });
    });
}

function editEntity(button, entityName) {
    const id = button.getAttribute('data-id');
    const currentName = button.getAttribute('data-name');
    const currentTitle = button.getAttribute('data-title');

    Swal.fire({
        title: 'Akademik Personel Düzenle',
        html: `<input id="swal-input1" class="swal2-input" value="${currentName}">` +
            `<input id="swal-input2" class="swal2-input" value="${currentTitle}">`,
        showCancelButton: true,
        confirmButtonText: 'Güncelle',
        cancelButtonText: 'İptal',
        preConfirm: () => {
            const ad = document.getElementById('swal-input1').value.trim();
            const unvan = document.getElementById('swal-input2').value.trim();
            if (!ad || !unvan) {
                Swal.showValidationMessage('Lütfen tüm alanları doldurunuz');
            }
            return { ad, unvan };
        }
    }).then(result => {
        if (result.isConfirmed) {
            updateEntity(id, result.value.ad, result.value.unvan, entityName);
        }
    });
}

function updateEntity(id, newName, newTitle, entityName) {
    fetch(`/${entityName}/Update`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Id: id, Ad: newName, Unvan: newTitle })
    })
        .then(response => response.json())
        .then(result => handleResponse(result, 'Güncelleme başarılı!'))
        .catch(handleError);
}

function handleResponse(result, successMessage) {
    if (result.success) {
        Swal.fire({
            icon: 'success',
            title: successMessage,
            text: result.message,
            toast: true,
            position: 'top-end',
            timer: 2000,
            showConfirmButton: false
        });
        setTimeout(() => location.reload(), 2000); // Sayfayı yenileyerek işlemi gösteriyoruz.
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
}

function handleError(error) {
    Swal.fire({
        icon: 'error',
        title: 'Hata!',
        text: 'Bir hata oluştu: ' + error,
        toast: true,
        position: 'top-end',
        timer: 3000,
        showConfirmButton: false
    });
}
