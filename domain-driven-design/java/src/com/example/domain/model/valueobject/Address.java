package com.example.domain.model.valueobject;

import java.util.Objects;

/**
 * 住所を表す値オブジェクト
 */
public final class Address {
    private final String streetAddress;
    private final String city;
    private final String state;
    private final String zipCode;
    private final String country;

    private Address(String streetAddress, String city, String state, String zipCode, String country) {
        this.streetAddress = streetAddress;
        this.city = city;
        this.state = state;
        this.zipCode = zipCode;
        this.country = country;
    }

    public static Address of(String streetAddress, String city, String state, String zipCode, String country) {
        Objects.requireNonNull(streetAddress, "Street address cannot be null");
        Objects.requireNonNull(city, "City cannot be null");
        Objects.requireNonNull(state, "State cannot be null");
        Objects.requireNonNull(zipCode, "Zip code cannot be null");
        Objects.requireNonNull(country, "Country cannot be null");

        if (streetAddress.isBlank()) {
            throw new IllegalArgumentException("Street address cannot be empty");
        }
        if (city.isBlank()) {
            throw new IllegalArgumentException("City cannot be empty");
        }
        if (state.isBlank()) {
            throw new IllegalArgumentException("State cannot be empty");
        }
        if (zipCode.isBlank()) {
            throw new IllegalArgumentException("Zip code cannot be empty");
        }
        if (country.isBlank()) {
            throw new IllegalArgumentException("Country cannot be empty");
        }

        return new Address(streetAddress, city, state, zipCode, country);
    }

    // 値オブジェクトは変更のためのメソッドを持つ場合、新しいインスタンスを返す
    public Address withStreetAddress(String streetAddress) {
        return new Address(streetAddress, this.city, this.state, this.zipCode, this.country);
    }

    public Address withCity(String city) {
        return new Address(this.streetAddress, city, this.state, this.zipCode, this.country);
    }

    public Address withState(String state) {
        return new Address(this.streetAddress, this.city, state, this.zipCode, this.country);
    }

    public Address withZipCode(String zipCode) {
        return new Address(this.streetAddress, this.city, this.state, zipCode, this.country);
    }

    public Address withCountry(String country) {
        return new Address(this.streetAddress, this.city, this.state, this.zipCode, country);
    }

    // 値オブジェクトのドメインロジック
    public boolean isSameCountryAs(Address other) {
        return this.country.equalsIgnoreCase(other.country);
    }

    public boolean isInternational(String baseCountry) {
        return !this.country.equalsIgnoreCase(baseCountry);
    }

    // ゲッター
    public String getStreetAddress() {
        return streetAddress;
    }

    public String getCity() {
        return city;
    }

    public String getState() {
        return state;
    }

    public String getZipCode() {
        return zipCode;
    }

    public String getCountry() {
        return country;
    }

    // 値オブジェクトの等価性は、全てのプロパティの値が等しいことで判断
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Address address = (Address) o;
        return Objects.equals(streetAddress, address.streetAddress) &&
                Objects.equals(city, address.city) &&
                Objects.equals(state, address.state) &&
                Objects.equals(zipCode, address.zipCode) &&
                Objects.equals(country, address.country);
    }

    @Override
    public int hashCode() {
        return Objects.hash(streetAddress, city, state, zipCode, country);
    }

    @Override
    public String toString() {
        return streetAddress + ", " + city + ", " + state + " " + zipCode + ", " + country;
    }

    // フォーマットされた住所を返す
    public String getFormattedAddress() {
        return String.format("%s\n%s, %s %s\n%s", 
                streetAddress, city, state, zipCode, country);
    }
}