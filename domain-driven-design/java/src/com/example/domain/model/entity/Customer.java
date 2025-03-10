package com.example.domain.model.entity;

import com.example.domain.model.valueobject.Address;
import com.example.domain.model.valueobject.CustomerId;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Objects;

/**
 * 顧客を表すエンティティ
 */
public class Customer {
    private final CustomerId id; // 識別子
    private String firstName;
    private String lastName;
    private String email;
    private String phoneNumber;
    private List<Address> addresses;
    private boolean active;
    private final LocalDateTime registeredAt;
    private LocalDateTime lastModifiedAt;

    // プライベートコンストラクタ - ファクトリメソッド経由で生成する
    private Customer(CustomerId id, String firstName, String lastName, String email, String phoneNumber) {
        this.id = id;
        this.firstName = firstName;
        this.lastName = lastName;
        this.email = email;
        this.phoneNumber = phoneNumber;
        this.addresses = new ArrayList<>();
        this.active = true;
        this.registeredAt = LocalDateTime.now();
        this.lastModifiedAt = LocalDateTime.now();
    }

    // ファクトリメソッド
    public static Customer create(String firstName, String lastName, String email, String phoneNumber) {
        validateNewCustomer(firstName, lastName, email);
        return new Customer(CustomerId.generateNew(), firstName, lastName, email, phoneNumber);
    }

    // 永続化からの復元用ファクトリメソッド
    public static Customer reconstitute(CustomerId id, String firstName, String lastName, 
                                       String email, String phoneNumber, List<Address> addresses, 
                                       boolean active, LocalDateTime registeredAt, LocalDateTime lastModifiedAt) {
        Customer customer = new Customer(id, firstName, lastName, email, phoneNumber);
        customer.addresses = new ArrayList<>(addresses);
        customer.active = active;
        customer.lastModifiedAt = lastModifiedAt;
        return customer;
    }

    // バリデーションロジック
    private static void validateNewCustomer(String firstName, String lastName, String email) {
        Objects.requireNonNull(firstName, "First name cannot be null");
        Objects.requireNonNull(lastName, "Last name cannot be null");
        Objects.requireNonNull(email, "Email cannot be null");

        if (firstName.isBlank()) {
            throw new IllegalArgumentException("First name cannot be empty");
        }
        if (lastName.isBlank()) {
            throw new IllegalArgumentException("Last name cannot be empty");
        }
        if (email.isBlank()) {
            throw new IllegalArgumentException("Email cannot be empty");
        }
        if (!isValidEmail(email)) {
            throw new IllegalArgumentException("Invalid email format");
        }
    }

    private static boolean isValidEmail(String email) {
        // 簡易的なメールアドレス検証
        return email.matches("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,6}$");
    }

    // ドメインロジック
    public void addAddress(Address address) {
        Objects.requireNonNull(address, "Address cannot be null");
        this.addresses.add(address);
        this.lastModifiedAt = LocalDateTime.now();
    }

    public void updateAddress(int index, Address newAddress) {
        Objects.requireNonNull(newAddress, "New address cannot be null");
        if (index < 0 || index >= addresses.size()) {
            throw new IndexOutOfBoundsException("Invalid address index");
        }
        this.addresses.set(index, newAddress);
        this.lastModifiedAt = LocalDateTime.now();
    }

    public void removeAddress(int index) {
        if (index < 0 || index >= addresses.size()) {
            throw new IndexOutOfBoundsException("Invalid address index");
        }
        if (addresses.size() == 1) {
            throw new IllegalStateException("Customer must have at least one address");
        }
        this.addresses.remove(index);
        this.lastModifiedAt = LocalDateTime.now();
    }

    public void updateContactInfo(String email, String phoneNumber) {
        Objects.requireNonNull(email, "Email cannot be null");
        if (email.isBlank()) {
            throw new IllegalArgumentException("Email cannot be empty");
        }
        if (!isValidEmail(email)) {
            throw new IllegalArgumentException("Invalid email format");
        }
        
        this.email = email;
        this.phoneNumber = phoneNumber;
        this.lastModifiedAt = LocalDateTime.now();
    }

    public void updateName(String firstName, String lastName) {
        Objects.requireNonNull(firstName, "First name cannot be null");
        Objects.requireNonNull(lastName, "Last name cannot be null");
        
        if (firstName.isBlank()) {
            throw new IllegalArgumentException("First name cannot be empty");
        }
        if (lastName.isBlank()) {
            throw new IllegalArgumentException("Last name cannot be empty");
        }
        
        this.firstName = firstName;
        this.lastName = lastName;
        this.lastModifiedAt = LocalDateTime.now();
    }

    public void activate() {
        this.active = true;
        this.lastModifiedAt = LocalDateTime.now();
    }

    public void deactivate() {
        this.active = false;
        this.lastModifiedAt = LocalDateTime.now();
    }

    // ゲッター
    public CustomerId getId() {
        return id;
    }

    public String getFirstName() {
        return firstName;
    }

    public String getLastName() {
        return lastName;
    }
    
    public String getFullName() {
        return firstName + " " + lastName;
    }

    public String getEmail() {
        return email;
    }

    public String getPhoneNumber() {
        return phoneNumber;
    }

    public List<Address> getAddresses() {
        return Collections.unmodifiableList(addresses);
    }

    public boolean isActive() {
        return active;
    }

    public LocalDateTime getRegisteredAt() {
        return registeredAt;
    }

    public LocalDateTime getLastModifiedAt() {
        return lastModifiedAt;
    }

    // エンティティの等価性は識別子によって判断
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Customer customer = (Customer) o;
        return Objects.equals(id, customer.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }

    @Override
    public String toString() {
        return "Customer{" +
                "id=" + id +
                ", firstName='" + firstName + '\'' +
                ", lastName='" + lastName + '\'' +
                ", email='" + email + '\'' +
                ", active=" + active +
                '}';
    }
}