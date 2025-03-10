/**
 * 商品更新リクエストDTO
 */
@Data
public class UpdateProductRequest {
    private String name;
    private String description;
    private Double price;
    private String currency;
    private Integer stockQuantity;
}