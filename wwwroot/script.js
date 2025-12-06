let selectedItems = [];
let editSelectedItems = [];

document.addEventListener('DOMContentLoaded', function() {
    loadAllInvoices();
});

function loadAllInvoices() {
    fetch('/api/invoice')
        .then(resp => resp.json())
        .then(data => {
            const tbody = document.getElementById('invoices-tbody');
            if (data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">No invoices found</td></tr>';
                return;
            }

            let html = '';
            data.forEach(invoice => {
                html += `<tr>`;
                html += `<td>${invoice.invoiceId}</td>`;
                html += `<td>${invoice.customerName}</td>`;
                html += `<td>$${invoice.total.toFixed(2)}</td>`;
                html += `<td>${invoice.date}</td>`;
                html += `<td>`;
                html += `<button onclick="viewInvoice(${invoice.invoiceId})" class="btn-view">View</button> `;
                html += `<button onclick="editInvoice(${invoice.invoiceId})" class="btn-edit">Edit</button> `;
                html += `<button onclick="deleteInvoice(${invoice.invoiceId})" class="btn-delete">Delete</button>`;
                html += `</td>`;
                html += `</tr>`;
            });
            tbody.innerHTML = html;
        })
        .catch(error => {
            document.getElementById('invoices-tbody').innerHTML = '<tr><td colspan="5" style="text-align:center;">Error loading invoices</td></tr>';
        });
}

function loadItems() {
    fetch('/api/item')
        .then(resp => resp.json())
        .then(data => {
            const select = document.getElementById('item-select');
            select.innerHTML = '<option value="">-- Select Item --</option>';
            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item.itemId;
                option.textContent = `${item.name} - $${item.price.toFixed(2)}`;
                select.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error loading items:', error);
        });
}

function showCreateInvoiceForm() {
    document.getElementById('create-invoice-section').style.display = 'block';
    document.getElementById('add-item-section').style.display = 'none';
    document.getElementById('edit-invoice-section').style.display = 'none';
    document.getElementById('view-invoice-section').style.display = 'none';
    document.getElementById('customer-name').value = '';
    document.getElementById('customer-contact').value = '';
    document.getElementById('customer-address').value = '';
    document.getElementById('item-quantity').value = '1';
    document.getElementById('create-message').textContent = '';
    selectedItems = [];
    document.getElementById('selected-items-ul').innerHTML = '';
    loadItems();
}

function hideCreateInvoiceForm() {
    document.getElementById('create-invoice-section').style.display = 'none';
    selectedItems = [];
}

function showAddItemForm() {
    document.getElementById('add-item-section').style.display = 'block';
    document.getElementById('create-invoice-section').style.display = 'none';
    document.getElementById('edit-invoice-section').style.display = 'none';
    document.getElementById('view-invoice-section').style.display = 'none';
    document.getElementById('item-name').value = '';
    document.getElementById('item-price').value = '';
    document.getElementById('item-message').textContent = '';
}

function hideAddItemForm() {
    document.getElementById('add-item-section').style.display = 'none';
}

function addItemToInvoiceList() {
    const select = document.getElementById('item-select');
    const quantityInput = document.getElementById('item-quantity');
    const selectedValue = select.value;
    const quantity = parseInt(quantityInput.value) || 1;
    
    if (!selectedValue) {
        alert('Please select an item');
        return;
    }

    if (quantity < 1) {
        alert('Quantity must be at least 1');
        return;
    }

    fetch('/api/item')
        .then(resp => resp.json())
        .then(data => {
            const item = data.find(i => i.itemId == selectedValue);
            if (item) {
                const existingItem = selectedItems.find(si => si.itemId == item.itemId);
                if (existingItem) {
                    existingItem.quantity = quantity;
                } else {
                    selectedItems.push({...item, quantity: quantity});
                }
                updateSelectedItemsList();
                select.value = '';
                quantityInput.value = '1';
            }
        })
        .catch(error => {
            console.error('Error loading item:', error);
        });
}

function updateSelectedItemsList() {
    const ul = document.getElementById('selected-items-ul');
    ul.innerHTML = '';
    selectedItems.forEach((item, index) => {
        const li = document.createElement('li');
        li.style.padding = '5px';
        li.style.borderBottom = '1px solid #ddd';
        const subtotal = item.price * item.quantity;
        li.innerHTML = `${item.name} - $${item.price.toFixed(2)} x ${item.quantity} = $${subtotal.toFixed(2)} <button onclick="removeSelectedItem(${index})" style="float:right; padding:2px 8px; background:#f44336; color:white; border:none; border-radius:3px; cursor:pointer;">Remove</button>`;
        ul.appendChild(li);
    });
}

function removeSelectedItem(index) {
    selectedItems.splice(index, 1);
    updateSelectedItemsList();
}

function viewInvoice(id) {
    fetch(`/api/invoice/${id}`)
        .then(resp => resp.json())
        .then(data => {
            document.getElementById('view-invoice-section').style.display = 'block';
            document.getElementById('create-invoice-section').style.display = 'none';
            document.getElementById('add-item-section').style.display = 'none';
            document.getElementById('edit-invoice-section').style.display = 'none';

            let html = '<div class="invoice-view">';
            html += `<h4>Invoice #${data.invoiceId}</h4>`;
            
            if (data.customer) {
                html += '<div class="customer-info">';
                html += `<p><strong>Customer:</strong> ${data.customer.name}</p>`;
                html += `<p><strong>Contact:</strong> ${data.customer.contact || 'N/A'}</p>`;
                html += `<p><strong>Address:</strong> ${data.customer.address || 'N/A'}</p>`;
                html += '</div>';
            }
            
            html += `<p><strong>Date:</strong> ${data.date}</p>`;
            html += '<table class="invoice-items-table">';
            html += '<thead><tr><th>Item</th><th>Quantity</th><th>Unit Price</th><th>Subtotal</th></tr></thead>';
            html += '<tbody>';
            
            if (data.items && data.items.length > 0) {
                data.items.forEach(item => {
                    const subtotal = item.price * (item.quantity || 1);
                    html += `<tr><td>${item.name}</td><td>${item.quantity || 1}</td><td>$${item.price.toFixed(2)}</td><td>$${subtotal.toFixed(2)}</td></tr>`;
                });
            } else {
                html += '<tr><td colspan="4">No items</td></tr>';
            }
            
            html += '</tbody>';
            html += `<tfoot><tr><td><strong>Total</strong></td><td><strong>$${data.total.toFixed(2)}</strong></td></tr></tfoot>`;
            html += '</table>';
            html += '</div>';
            
            document.getElementById('view-invoice-content').innerHTML = html;
        })
        .catch(error => {
            alert('Error loading invoice details');
        });
}

function hideViewForm() {
    document.getElementById('view-invoice-section').style.display = 'none';
}

function createInvoice() {
    const customerName = document.getElementById('customer-name').value;
    const customerContact = document.getElementById('customer-contact').value;
    const customerAddress = document.getElementById('customer-address').value;
    
    if (!customerName) {
        document.getElementById('create-message').textContent = 'Please enter customer name';
        return;
    }

    if (selectedItems.length === 0) {
        document.getElementById('create-message').textContent = 'Please select at least one item';
        return;
    }

    const invoiceItems = selectedItems.map(item => ({
        itemId: item.itemId,
        quantity: item.quantity
    }));

    fetch('/api/invoice', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
            customerId: 0,
            customerName: customerName,
            customerContact: customerContact,
            customerAddress: customerAddress,
            items: invoiceItems
        })
    })
    .then(resp => resp.json())
    .then(data => {
        document.getElementById('create-message').textContent = 'Invoice created successfully!';
        document.getElementById('customer-name').value = '';
        document.getElementById('customer-contact').value = '';
        document.getElementById('customer-address').value = '';
        selectedItems = [];
        document.getElementById('selected-items-ul').innerHTML = '';
        setTimeout(() => {
            hideCreateInvoiceForm();
            loadAllInvoices();
        }, 1000);
    })
    .catch(error => {
        document.getElementById('create-message').textContent = 'Error creating invoice';
    });
}

function addItemToCatalog() {
    const itemName = document.getElementById('item-name').value;
    const itemPrice = parseFloat(document.getElementById('item-price').value);

    if (!itemName || !itemPrice) {
        document.getElementById('item-message').textContent = 'Please fill all fields';
        return;
    }

    fetch('/api/item', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: itemName, price: itemPrice })
    })
    .then(resp => resp.json())
    .then(data => {
        document.getElementById('item-message').textContent = 'Item added successfully!';
        document.getElementById('item-name').value = '';
        document.getElementById('item-price').value = '';
        setTimeout(() => {
            hideAddItemForm();
            loadAllInvoices();
        }, 1000);
    })
    .catch(error => {
        document.getElementById('item-message').textContent = 'Error adding item';
    });
}

function editInvoice(id) {
    fetch(`/api/invoice/${id}`)
        .then(resp => resp.json())
        .then(data => {
            document.getElementById('edit-invoice-section').style.display = 'block';
            document.getElementById('create-invoice-section').style.display = 'none';
            document.getElementById('add-item-section').style.display = 'none';
            document.getElementById('view-invoice-section').style.display = 'none';
            document.getElementById('edit-invoice-id').value = id;
            document.getElementById('edit-invoice-number').textContent = id;
            
            if (data.customer) {
                document.getElementById('edit-customer-name').value = data.customer.name;
                document.getElementById('edit-customer-contact').value = data.customer.contact || '';
                document.getElementById('edit-customer-address').value = data.customer.address || '';
            }
            
            editSelectedItems = [];
            if (data.items && data.items.length > 0) {
                data.items.forEach(item => {
                    editSelectedItems.push({
                        itemId: item.itemId,
                        name: item.name,
                        price: item.price,
                        quantity: item.quantity || 1
                    });
                });
            }
            
            updateEditSelectedItemsList();
            loadItemsForEdit();
            document.getElementById('edit-message').textContent = '';
        })
        .catch(error => {
            alert('Error loading invoice');
        });
}

function loadItemsForEdit() {
    fetch('/api/item')
        .then(resp => resp.json())
        .then(data => {
            const select = document.getElementById('edit-item-select');
            select.innerHTML = '<option value="">-- Select Item --</option>';
            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item.itemId;
                option.textContent = `${item.name} - $${item.price.toFixed(2)}`;
                select.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error loading items:', error);
        });
}

function addItemToEditList() {
    const select = document.getElementById('edit-item-select');
    const quantityInput = document.getElementById('edit-item-quantity');
    const selectedValue = select.value;
    const quantity = parseInt(quantityInput.value) || 1;
    
    if (!selectedValue) {
        alert('Please select an item');
        return;
    }

    if (quantity < 1) {
        alert('Quantity must be at least 1');
        return;
    }

    fetch('/api/item')
        .then(resp => resp.json())
        .then(data => {
            const item = data.find(i => i.itemId == selectedValue);
            if (item) {
                const existingItem = editSelectedItems.find(si => si.itemId == item.itemId);
                if (existingItem) {
                    existingItem.quantity = quantity;
                } else {
                    editSelectedItems.push({...item, quantity: quantity});
                }
                updateEditSelectedItemsList();
                select.value = '';
                quantityInput.value = '1';
            }
        })
        .catch(error => {
            console.error('Error loading item:', error);
        });
}

function updateEditSelectedItemsList() {
    const ul = document.getElementById('edit-selected-items-ul');
    ul.innerHTML = '';
    editSelectedItems.forEach((item, index) => {
        const li = document.createElement('li');
        li.style.padding = '5px';
        li.style.borderBottom = '1px solid #ddd';
        const subtotal = item.price * item.quantity;
        li.innerHTML = `${item.name} - $${item.price.toFixed(2)} x ${item.quantity} = $${subtotal.toFixed(2)} <button onclick="removeEditSelectedItem(${index})" style="float:right; padding:2px 8px; background:#f44336; color:white; border:none; border-radius:3px; cursor:pointer;">Remove</button>`;
        ul.appendChild(li);
    });
}

function removeEditSelectedItem(index) {
    editSelectedItems.splice(index, 1);
    updateEditSelectedItemsList();
}

function hideEditForm() {
    document.getElementById('edit-invoice-section').style.display = 'none';
    editSelectedItems = [];
}

function updateInvoice() {
    const invoiceId = parseInt(document.getElementById('edit-invoice-id').value);
    const customerName = document.getElementById('edit-customer-name').value;
    const customerContact = document.getElementById('edit-customer-contact').value;
    const customerAddress = document.getElementById('edit-customer-address').value;

    if (!customerName) {
        document.getElementById('edit-message').textContent = 'Please enter customer name';
        return;
    }

    if (editSelectedItems.length === 0) {
        document.getElementById('edit-message').textContent = 'Please add at least one item';
        return;
    }

    const invoiceItems = editSelectedItems.map(item => ({
        itemId: item.itemId,
        quantity: item.quantity
    }));

    fetch(`/api/invoice/${invoiceId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
            customerName: customerName,
            customerContact: customerContact,
            customerAddress: customerAddress,
            items: invoiceItems
        })
    })
    .then(resp => resp.json())
    .then(data => {
        document.getElementById('edit-message').textContent = 'Invoice updated successfully!';
        setTimeout(() => {
            hideEditForm();
            loadAllInvoices();
        }, 1000);
    })
    .catch(error => {
        document.getElementById('edit-message').textContent = 'Error updating invoice';
    });
}

function deleteInvoice(id) {
    if (!confirm('Are you sure you want to delete this invoice?')) {
        return;
    }

    fetch(`/api/invoice/${id}`, {
        method: 'DELETE'
    })
    .then(resp => resp.json())
    .then(data => {
        loadAllInvoices();
    })
    .catch(error => {
        alert('Error deleting invoice');
    });
}
