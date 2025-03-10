/**
 * 商品レスポンスDTO
 */
@Data
public class ProductResponse {
    private final String productId;
    private final String name;
    private final String description;
    private final Double price;
    private final String currency;
    private final Integer stockQuantity;
    private final Boolean active;
}