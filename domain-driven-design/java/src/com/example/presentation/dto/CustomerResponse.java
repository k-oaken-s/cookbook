/**
 * 顧客レスポンスDTO
 */
@Data
public class CustomerResponse {
    private final String customerId;
    private final String firstName;
    private final String lastName;
    private final String email;
    private final String phoneNumber;
    private final Boolean active;
}