$(document).ready(function () {
    getOperationClaims();
    getUserOperationClaims();
    getUsers();
    getOperationClaimsForSelect();
    updateUserTable();
});

// Operation Claims işlemleri
function getOperationClaims() {
    $.get('/management/operationclaim/getall', function (result) {
        if (result.success) {
            const table = $('#operationClaimsTable').empty();
            result.data.forEach(claim => {
                table.append(`
                    <tr>
                        <td>${claim.id}</td>
                        <td>${claim.name}</td>
                        <td>
                            <button class="btn btn-sm btn-danger" onclick="deleteOperationClaim(${claim.id})">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                `);
            });
        } else {
            showError(result.message);
        }
    }).fail(() => showError('Veriler getirilirken bir hata oluştu.'));
}

function getOperationClaimsForSelect() {
    $.get('/management/operationclaim/getall', function (result) {
        if (result.success) {
            const select = $('#operationClaimId').empty().append('<option value="">Yetki Seçin</option>');
            result.data.forEach(claim => {
                select.append(`<option value="${claim.id}">${claim.name}</option>`);
            });
        }
    });
}

function addOperationClaim() {
    const name = $('#operationClaimName').val();
    $.post('/management/operationclaim/add', JSON.stringify({ name }), function (result) {
        if (result.success) {
            $('#addOperationClaimModal').modal('hide');
            showSuccess('Yetki başarıyla eklendi.');
            getOperationClaims();
            getOperationClaimsForSelect();
        } else {
            showError(result.message);
        }
    }).fail(xhr => showError(xhr.responseText));
}

function deleteOperationClaim(id) {
    confirmAction('Bu yetki kalıcı olarak silinecek!', () => {
        $.post(`/management/operationclaim/delete/${id}`, function (result) {
            if (result.success) {
                showSuccess('Yetki başarıyla silindi.');
                getOperationClaims();
                getOperationClaimsForSelect();
            } else {
                showError(result.message);
            }
        }).fail(xhr => showError(xhr.responseText));
    });
}

// User Operation Claims işlemleri
function getUserOperationClaims() {
    $.get('/management/useroperationclaim/getall', function (result) {
        if (result.success) {
            const table = $('#userOperationClaimsTable').empty();
            result.data.forEach(claim => {
                $.when(
                    $.get(`/management/user/getbyid/${claim.userId}`),
                    $.get(`/management/operationclaim/getbyid/${claim.operationClaimId}`)
                ).done((userRes, claimRes) => {
                    if (userRes[0].success && claimRes[0].success) {
                        table.append(`
                            <tr>
                                <td>${claim.id}</td>
                                <td>${userRes[0].data.firstName} | ${userRes[0].data.userName} (${userRes[0].data.eMail})</td>
                                <td>${claimRes[0].data.name}</td>
                                <td>
                                    <button class="btn btn-sm btn-danger" onclick="deleteUserOperationClaim(${claim.id})">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </td>
                            </tr>
                        `);
                    }
                });
            });
        } else {
            showError(result.message);
        }
    }).fail(() => showError('Veriler getirilirken bir hata oluştu.'));
}

function getUsers() {
    $.get('/management/user/getall', function (result) {
        if (result.success) {
            const select = $('#userId').empty().append('<option value="">Kullanıcı Seçin</option>');
            result.data.forEach(user => {
                select.append(`<option value="${user.id}">${user.userName} (${user.eMail})</option>`);
            });
        }
    });
}

function addUserOperationClaim() {
    const userId = Number($('#userId').val());
    const operationClaimId = Number($('#operationClaimId').val());
    if (!userId || !operationClaimId) return showError('Lütfen kullanıcı ve yetki seçiniz.');
    $.post('/management/useroperationclaim/add', JSON.stringify({ userId, operationClaimId }), function (result) {
        if (result.success) {
            $('#addUserOperationClaimModal').modal('hide');
            showSuccess('Kullanıcı yetkisi başarıyla eklendi.');
            getUserOperationClaims();
        } else {
            showError(result.message);
        }
    }).fail(xhr => showError(xhr.responseText));
}

function deleteUserOperationClaim(id) {
    confirmAction('Bu kullanıcı yetkisi kalıcı olarak silinecek!', () => {
        $.get(`/management/useroperationclaim/delete/${id}`, function () {
            showSuccess('Kullanıcı yetkisi başarıyla silindi.');
            getUserOperationClaims();
        }).fail(xhr => showError(xhr.responseText));
    });
}

function deleteUser(id) {
    confirmAction('Bu kullanıcı kalıcı olarak silinecek!', () => {
        $.get(`/management/user/delete/${id}`, function (response) {
            if (response.success) {
                showSuccess(response.message);
                getUsers();
            } else {
                showError(response.message);
            }
        }).fail(xhr => showError(xhr.responseText));
    });
}

function updateUserTable() {
    $.get('/management/user/getall', function (result) {
        console.log(result);
        if (result.success) {
            const table = $('#usersTable').empty();
            result.data.forEach(user => {
                table.append(`
                    <tr>
                        <td>${user.id}</td>
                        <td>${user.userName}</td>
                        <td>${user.firstName} ${user.lastName}</td>
                        <td>${user.eMail}</td>
                        <td>
                            <button class="btn btn-sm btn-danger" onclick="deleteUser(${user.id})">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                `);
            });
        } else {
            showError(result.message);
        }
    });
}

// Yardımcı Fonksiyonlar
function showError(message) {
    Swal.fire({ title: 'Hata!', text: message || 'Bir hata oluştu.', icon: 'error' });
}

function showSuccess(message) {
    Swal.fire({ title: 'Başarılı!', text: message, icon: 'success', timer: 2000, showConfirmButton: false });
}

function confirmAction(message, callback) {
    Swal.fire({
        title: 'Emin misiniz?',
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Evet, sil!',
        cancelButtonText: 'İptal'
    }).then(result => { if (result.isConfirmed) callback(); });
}

$(document).ready(getUsers);
